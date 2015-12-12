using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using WoWClassic.Cluster;
using WoWClassic.Common;
using WoWClassic.Common.Constants;
using WoWClassic.Common.Crypto;
using WoWClassic.Common.DataStructure;
using WoWClassic.Common.Log;
using WoWClassic.Common.Packets;

namespace WoWClassic.Login.AuthServer
{
    public class AuthConnection
    {
        public AuthConnection(Socket socket, AuthServer server)
        {
            m_Socket = socket;
            m_Server = server;

            m_CommandHandlers = RegisterHandlers(this);

            OnAccept();
        }

        private readonly Socket m_Socket;
        private readonly AuthServer m_Server;

        private C_AuthLogonChallenge m_ALC;
        private C_AuthLogonProof m_ALP;
        private SRP m_SRP;

        public string Username { get { return m_ALC.Identifier; } }
        public bool IsAuthenticated { get { return m_SRP != null && m_SRP.ClientProof == m_SRP.GenerateClientProof(); } }

        private readonly Dictionary<AuthOpcodes, CommandHandler> m_CommandHandlers;

        private Thread m_ThreadReceive;
        private byte[] m_RecvBuffer = new byte[1024];

        #region Packet Handler Reflection

        private static Dictionary<AuthOpcodes, CommandHandler> RegisterHandlers(object instance)
        {
            var ret = new Dictionary<AuthOpcodes, CommandHandler>();
            var type = instance.GetType();
            foreach (var method in type.GetMethods())
            {
                var attr = method.GetCustomAttributes(typeof(PacketHandlerAttribute), false).Cast<PacketHandlerAttribute>().FirstOrDefault();
                if (attr == null) continue;
                ret.Add((AuthOpcodes)attr.PacketId, (CommandHandler)method.CreateDelegate(typeof(CommandHandler), instance));
            }
            return ret;
        }

        #endregion

        public void OnAccept()
        {
            Console.WriteLine("Accepting connection from {0}", ((IPEndPoint)m_Socket.RemoteEndPoint).Address);

            m_ThreadReceive = new Thread(OnReceive);
            m_ThreadReceive.Start();
        }

        public void OnReceive()
        {
            int bytesRead;
            while ((bytesRead = m_Socket.Receive(m_RecvBuffer)) > 0)
            {
                var buffer = new byte[bytesRead];
                Buffer.BlockCopy(m_RecvBuffer, 0, buffer, 0, bytesRead);

                using (var ms = new MemoryStream(buffer))
                using (var br = new BinaryReader(ms))
                {
                    var command = (AuthOpcodes)br.ReadByte();

                    Log.WriteLine(AuthLogTypes.Packets, $"<- {command}({bytesRead})\n\t{string.Join(" ", buffer.Select(b => b.ToString("X2")))}");
                    if (!m_CommandHandlers.ContainsKey(command) || !m_CommandHandlers[command](br))
                        Log.WriteLine(AuthLogTypes.Packets, $"Failed to handle command {command}");
                }
            }
            m_Server.Clients.Remove(this);
            Console.WriteLine("Dropped connection from {0}", ((IPEndPoint)m_Socket.RemoteEndPoint).Address);
        }

        private void SendPacket(AuthOpcodes opcode, byte[] data)
        {
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                bw.Write((byte)opcode);
                bw.Write(data);

                var packet = ms.ToArray();
                Log.WriteLine(AuthLogTypes.Packets, $"-> {opcode}({packet.Length}):\n\t{string.Join(" ", packet.Select(b => b.ToString("X2")))}");
                m_Socket.Send(packet);
            }
        }

        #region AuthLogonChallenge

        private class C_AuthLogonChallenge
        {
            public byte Error;
            [PacketBigEndian]
            public ushort Size;
            [PacketArrayLength(4), PacketArrayReverse]
            public byte[] GameName;
            public byte Version1;
            public byte Version2;
            public byte Version3;
            [PacketBigEndian]
            public ushort Build;
            [PacketArrayLength(4), PacketArrayReverse]
            public byte[] Platform;
            [PacketArrayLength(4), PacketArrayReverse]
            public byte[] OS;
            [PacketArrayLength(4), PacketArrayReverse]
            public byte[] Country;
            [PacketBigEndian]
            public uint TimezoneBias;
            [PacketBigEndian]
            public uint IP;
            [PacketString(StringTypes.PrefixedLength)]
            public string Identifier;
        }

        private class S_AuthLogonChallenge
        {
            public byte Error;
            public byte unk1;
            public byte[] ServerEphemeral;
            public byte GeneratorLength;
            public byte[] Generator;
            public byte ModulusLength;
            public byte[] Modulus;
            public byte[] Salt;
            public byte[] unk2;
        }

        [PacketHandler(AuthOpcodes.AuthLogonChallenge)]
        public bool HandleAuthLogonChallenge(BinaryReader br)
        {
            m_ALC = PacketHelper.Parse<C_AuthLogonChallenge>(br);

            //Console.WriteLine("<- {0} connecting ({1}.{2}.{3}.{4})", m_ALC.Identifier, m_ALC.Version1, m_ALC.Version2, m_ALC.Version3, m_ALC.Build);

            // Check ban
            // SendPacket(AuthOpcodes.AuthLogonChallenge, PacketHelper.Build(new S_AuthLogonChallenge { Error = (byte)AuthResult.Banned }));

            // Check suspend
            // SendPacket(AuthOpcodes.AuthLogonChallenge, PacketHelper.Build(new S_AuthLogonChallenge { Error = (byte)AuthResult.Suspended }));

            // Account doesn't exist!
            if (!LoginService.ExistsAccount(m_ALC.Identifier))
            {
                SendPacket(AuthOpcodes.AuthLogonChallenge, PacketHelper.Build(new S_AuthLogonChallenge { Error = (byte)AuthResult.UnknownAccount }));
                return true;
            }
            m_SRP = LoginService.GetAccountSecurity(m_ALC.Identifier);

            SendPacket(AuthOpcodes.AuthLogonChallenge, PacketHelper.Build(new S_AuthLogonChallenge
            {
                Error = (byte)AuthResult.Success,
                unk1 = 0,
                ServerEphemeral = m_SRP.ServerEphemeral.ToProperByteArray().Pad(32),
                GeneratorLength = 1,
                Generator = m_SRP.Generator.ToByteArray(),
                ModulusLength = 32,
                Modulus = m_SRP.Modulus.ToProperByteArray().Pad(32),
                Salt = m_SRP.Salt.ToProperByteArray().Pad(32),
                unk2 = new byte[17]
            }));

            return true;
        }

        #endregion

        public class C_AuthLogonProof
        {
            [PacketArrayLength(32)]
            public byte[] A;
            [PacketArrayLength(20)]
            public byte[] M1;
            [PacketArrayLength(20)]
            public byte[] CRC;
            public byte nKeys;
            public byte SecurityFlags;
        }

        public class S_AuthLogonProof
        {
            public byte Error;
            public byte[] M2;
            public uint unk2;
        }

        [PacketHandler(AuthOpcodes.AuthLogonProof)]
        public bool HandleLogonProof(BinaryReader br)
        {
            m_ALP = PacketHelper.Parse<C_AuthLogonProof>(br);


            m_SRP.ClientEphemeral = m_ALP.A.ToPositiveBigInteger();
            m_SRP.ClientProof = m_ALP.M1.ToPositiveBigInteger();

            // Check versions
            if (m_ALC.Build != 5875)
            {
                SendPacket(AuthOpcodes.AuthLogonProof, PacketHelper.Build(new S_AuthLogonProof { Error = (byte)AuthResult.InvalidVersion }));
                return true;
            }

            // Check password
            if (!IsAuthenticated) // TODO: Send response
            {
                // Increment password fail counter?
                SendPacket(AuthOpcodes.AuthLogonProof, PacketHelper.Build(new S_AuthLogonProof { Error = (byte)AuthResult.IncorrectPassword }));
                return true;
            }

            LoginService.UpdateSessionKey(m_ALC.Identifier, m_SRP.SessionKey.ToProperByteArray());
            SendPacket(AuthOpcodes.AuthLogonProof, PacketHelper.Build(new S_AuthLogonProof
            {
                Error = (byte)AuthResult.Success,
                M2 = m_SRP.ServerProof.ToByteArray().Pad(20),
                unk2 = 0
            }));
            return true;
        }


        // https://github.com/cmangos/mangos-classic/blob/master/src/realmd/AuthSocket.cpp#L747-L852
        [PacketHandler(AuthOpcodes.AuthReconnectChallenge)]
        public bool HandleReconnectChallenge(BinaryReader br)
        {
            return false;
        }

        [PacketHandler(AuthOpcodes.AuthReconnectProof)]
        public bool HandleReconnectProof(BinaryReader br)
        {
            return false;
        }

        public class S_RealmList
        {
            private uint unk0 = 0;
            public byte RealmCount;
            public Realm[] Realms;
            private ushort unk1 = 2;
        }

        // https://github.com/cmangos/mangos-classic/blob/master/src/realmd/AuthSocket.cpp#L855-L954
        [PacketHandler(AuthOpcodes.RealmList)]
        public bool HandleRealmlist(BinaryReader br)
        {
            var realms = m_Server.Service.RealmStates.Select(r => r.Realm);

            var data = PacketHelper.Build(new S_RealmList
            {
                RealmCount = (byte)realms.Count(),
                Realms = realms.ToArray()
            });


            SendPacket(AuthOpcodes.RealmList, BitConverter.GetBytes((ushort)data.Length).Concat(data).ToArray());

            return true;
        }
    }
}
