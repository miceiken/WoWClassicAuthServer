using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using WoWClassic.Common;
using WoWClassic.Common.Constants;
using WoWClassic.Common.Crypto;
using WoWClassic.Common.Protocol;
using WoWClassic.Common.Packets;
using System.Security.Cryptography;
using WoWClassic.Cluster;
using System.Runtime.InteropServices;

namespace WoWClassic.Gateway
{
    public class GatewayConnection
    {
        public GatewayConnection(Socket socket, GatewayServer server)
        {
            m_Socket = socket;
            m_Server = server;

            m_CommandHandlers = RegisterHandlers(this);

            OnAccept();
        }

        private static Random s_Rnd = new Random();

        private readonly Socket m_Socket;
        private readonly GatewayServer m_Server;

        private Thread m_ThreadReceive;
        private byte[] m_RecvBuffer = new byte[1024];

        private int m_Seed = s_Rnd.Next();
        private AuthCrypt m_Crypt;

        private readonly Dictionary<WorldOpcodes, CommandHandler> m_CommandHandlers;

        #region Packet Handler Reflection

        private static Dictionary<WorldOpcodes, CommandHandler> RegisterHandlers(object instance)
        {
            var ret = new Dictionary<WorldOpcodes, CommandHandler>();
            var type = instance.GetType();
            foreach (var method in type.GetMethods())
            {
                var attr = method.GetCustomAttributes(typeof(PacketHandlerAttribute), false).Cast<PacketHandlerAttribute>().FirstOrDefault();
                if (attr == null) continue;
                ret.Add((WorldOpcodes)attr.PacketId, (CommandHandler)method.CreateDelegate(typeof(CommandHandler), instance));
            }
            return ret;
        }

        #endregion

        #region Socket

        public void OnAccept()
        {
            Console.WriteLine("Accepting connection from {0}", ((IPEndPoint)m_Socket.RemoteEndPoint).Address);

            m_ThreadReceive = new Thread(OnReceive);
            m_ThreadReceive.Start();
        }

        public void OnReceive()
        {
            HandleAcceptedConnection();

            int bytesRead;
            while ((bytesRead = m_Socket.Receive(m_RecvBuffer, m_RecvBuffer.Length, SocketFlags.None)) > 0)
            {
                var buffer = new byte[bytesRead];
                Buffer.BlockCopy(m_RecvBuffer, 0, buffer, 0, bytesRead);

                using (var ms = new MemoryStream(buffer))
                using (var br = new BinaryReader(ms))
                {
                    var header = new WorldPacketHeader(m_Crypt, br);
                    Console.WriteLine($"<- {header.Opcode}({header.Length}|{buffer.Length})");
                    if (m_CommandHandlers.ContainsKey(header.Opcode))
                    {
                        if (!m_CommandHandlers[header.Opcode](br, header.Length - 6))
                            Console.WriteLine($"Failed to handle command {header.Opcode}");
                    }
                }
            }
            m_Server.Clients.Remove(this);
            Console.WriteLine("Dropped connection from {0}", ((IPEndPoint)m_Socket.RemoteEndPoint).Address);
        }

        private void SendPacket(WorldOpcodes opcode, byte[] data)
        {
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                bw.Write(((ushort)(data.Length + 2)).SwitchEndian());
                bw.Write((ushort)opcode);
                bw.Write(data);

                var packet = ms.ToArray();
                m_Crypt?.Encrypt(packet);
                Console.WriteLine($"-> {opcode}({data.Length}|{packet.Length})");
                m_Socket.Send(packet);
            }
        }

        #endregion

        #region Packets

        [StructLayout(LayoutKind.Sequential)]
        private class SMSG_AUTH_CHALLENGE
        {
            public int Seed;
        }

        private void HandleAcceptedConnection()
        {
            SendPacket(WorldOpcodes.SMSG_AUTH_CHALLENGE, PacketHelper.Build(new SMSG_AUTH_CHALLENGE
            {
                Seed = m_Seed
            }));
        }

        #region CMSG_AUTH_SESSION

        [StructLayout(LayoutKind.Sequential)]
        private class CMSG_AUTH_SESSION
        {
            public uint Build;
            private uint Unknown;
            [StringParse(StringTypes.CString)]
            public string Account;
            public uint ClientSeed;
            [ArrayLength(20)]
            public byte[] ClientDigest;
        }

        [StructLayout(LayoutKind.Sequential)]
        private class SMSG_AUTH_RESPONSE
        {
            public byte Response;
            public uint BillingTimeRemaining;
            public byte BillingPlanFlags;
            public uint BillingTimeRested;
        }

        [PacketHandler(WorldOpcodes.CMSG_AUTH_SESSION)]
        public bool HandleAuthSession(BinaryReader br, int bytesRead)
        {
            var pkt = PacketHelper.Parse<CMSG_AUTH_SESSION>(br);

            // TODO: verify build

            m_Crypt = new AuthCrypt(LoginService.GetSessionKey(pkt.Account));

            var serverDigest = ComputeHash(Encoding.ASCII.GetBytes(pkt.Account),
                new byte[] { 0, 0, 0, 0 },
                BitConverter.GetBytes(pkt.ClientSeed),
                BitConverter.GetBytes(m_Seed),
                m_Crypt.SessionKey);
            if (!serverDigest.SequenceEqual(pkt.ClientDigest))
                return false;

            SendPacket(WorldOpcodes.SMSG_AUTH_RESPONSE, PacketHelper.Build(new SMSG_AUTH_RESPONSE
            {
                Response = (byte)ResponseCodes.AUTH_OK,
                BillingTimeRemaining = 0,
                BillingPlanFlags = 0,
                BillingTimeRested = 0
            }));

            return true;
        }

        #endregion

        #region CMSG_CHAR_ENUM

        [StructLayout(LayoutKind.Sequential)]
        private class CMSG_CHAR_ENUM
        {
        }

        [StructLayout(LayoutKind.Sequential)]
        private class SMSG_CHAR_ENUM
        {
            public byte Length;
            public CharEnumEntry[] Characters;
        }

        [StructLayout(LayoutKind.Sequential)]
        private class CharEnumEntry
        {
            public uint GUID;
            [StringParse(StringTypes.CString)]
            public string Name;
            public byte Race;
            public byte Class;
            public byte Gender;
            public uint PlayerBytes1;
            public uint PlayerBytes2;
            public float X, Y, Z;
            public uint CharacterFlags;
            public byte FirstLogin;

            public uint PetDisplayId;
            public uint PetLevel;
            public uint PetFamily;

            // Equipment stuff
            //public uint[] DisplayInfoId;
            //public byte[] InventoryType;

            public uint FirstBagDisplayId;
            public byte FirstBagInventoryType;
        }



        [PacketHandler(WorldOpcodes.CMSG_CHAR_ENUM)]
        public bool HandleCharEnum(BinaryReader br, int bytesRead)
        {
            var pkt = PacketHelper.Parse<CMSG_CHAR_ENUM>(br);


            SendPacket(WorldOpcodes.SMSG_CHAR_ENUM, PacketHelper.Build(new SMSG_CHAR_ENUM
            {
            }));

            return true;
        }

        #endregion

        #endregion

        private static byte[] ComputeHash(params byte[][] args)
        {
            using (var sha = SHA1.Create())
                return sha.ComputeHash(args.SelectMany(b => b).ToArray());
        }
    }
}