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

namespace WoWClassic.Login
{
    public static class AuthHandler
    {

        static AuthHandler()
        {
            PacketHandlers = new Dictionary<AuthOpcodes, StaticCommandHandler<AuthConnection>>();
            foreach (var method in typeof(AuthHandler).GetMethods())
            {
                var attr = method.GetCustomAttributes(typeof(PacketHandlerAttribute), false).Cast<PacketHandlerAttribute>().FirstOrDefault();
                if (attr == null) continue;
                PacketHandlers.Add((AuthOpcodes)attr.PacketId, (StaticCommandHandler<AuthConnection>)method.CreateDelegate(typeof(StaticCommandHandler<AuthConnection>), null));
            }
        }

        public static Dictionary<AuthOpcodes, StaticCommandHandler<AuthConnection>> PacketHandlers { get; private set; }

        #region AuthLogonChallenge

        public class C_AuthLogonChallenge
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
        public static bool HandleAuthLogonChallenge(AuthConnection client, BinaryReader br)
        {
            client.LogonChallenge = PacketHelper.Parse<C_AuthLogonChallenge>(br);

            //Console.WriteLine("<- {0} connecting ({1}.{2}.{3}.{4})", client.LogonChallenge.Identifier, client.LogonChallenge.Version1, client.LogonChallenge.Version2, client.LogonChallenge.Version3, client.LogonChallenge.Build);

            // Check ban
            // client.SendPacket(AuthOpcodes.AuthLogonChallenge, PacketHelper.Build(new S_AuthLogonChallenge { Error = (byte)AuthResult.Banned }));

            // Check suspend
            // client.SendPacket(AuthOpcodes.AuthLogonChallenge, PacketHelper.Build(new S_AuthLogonChallenge { Error = (byte)AuthResult.Suspended }));

            // Account doesn't exist!
            if (!LoginService.ExistsAccount(client.LogonChallenge.Identifier))
            {
                client.SendPacket(AuthOpcodes.AuthLogonChallenge, PacketHelper.Build(new S_AuthLogonChallenge { Error = (byte)AuthResult.UnknownAccount }));
                return true;
            }
            client.SRP = LoginService.GetAccountSecurity(client.LogonChallenge.Identifier);

            client.SendPacket(AuthOpcodes.AuthLogonChallenge, PacketHelper.Build(new S_AuthLogonChallenge
            {
                Error = (byte)AuthResult.Success,
                unk1 = 0,
                ServerEphemeral = client.SRP.ServerEphemeral.ToProperByteArray().Pad(32),
                GeneratorLength = 1,
                Generator = client.SRP.Generator.ToByteArray(),
                ModulusLength = 32,
                Modulus = client.SRP.Modulus.ToProperByteArray().Pad(32),
                Salt = client.SRP.Salt.ToProperByteArray().Pad(32),
                unk2 = new byte[17]
            }));

            return true;
        }

        #endregion

        #region AuthLogonProof

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
        public static bool HandleLogonProof(AuthConnection client, BinaryReader br)
        {
            client.LogonProof = PacketHelper.Parse<C_AuthLogonProof>(br);


            client.SRP.ClientEphemeral = client.LogonProof.A.ToPositiveBigInteger();
            client.SRP.ClientProof = client.LogonProof.M1.ToPositiveBigInteger();

            // Check versions
            if (client.LogonChallenge.Build != 5875)
            {
                client.SendPacket(AuthOpcodes.AuthLogonProof, PacketHelper.Build(new S_AuthLogonProof { Error = (byte)AuthResult.InvalidVersion }));
                return true;
            }

            // Check password
            if (!client.IsAuthenticated) // TODO: Send response
            {
                // Increment password fail counter?
                client.SendPacket(AuthOpcodes.AuthLogonProof, PacketHelper.Build(new S_AuthLogonProof { Error = (byte)AuthResult.IncorrectPassword }));
                return true;
            }

            LoginService.UpdateSessionKey(client.LogonChallenge.Identifier, client.SRP.SessionKey.ToProperByteArray());
            client.SendPacket(AuthOpcodes.AuthLogonProof, PacketHelper.Build(new S_AuthLogonProof
            {
                Error = (byte)AuthResult.Success,
                M2 = client.SRP.ServerProof.ToByteArray().Pad(20),
                unk2 = 0
            }));
            return true;
        }

        #endregion

        #region AuthReconnectChallenge

        // https://github.com/cmangos/mangos-classic/blob/master/src/realmd/AuthSocket.cpp#L747-L852
        [PacketHandler(AuthOpcodes.AuthReconnectChallenge)]
        public static bool HandleReconnectChallenge(AuthConnection client, BinaryReader br)
        {
            return false;
        }

        #endregion

        #region AuthReconnectProof

        [PacketHandler(AuthOpcodes.AuthReconnectProof)]
        public static bool HandleReconnectProof(AuthConnection client, BinaryReader br)
        {
            return false;
        }

        #endregion

        #region Realmlist

        public class S_RealmList
        {
            private uint unk0 = 0;
            public byte RealmCount;
            public Realm[] Realms;
            private ushort unk1 = 2;
        }

        // https://github.com/cmangos/mangos-classic/blob/master/src/realmd/AuthSocket.cpp#L855-L954
        [PacketHandler(AuthOpcodes.RealmList)]
        public static bool HandleRealmlist(AuthConnection client, BinaryReader br)
        {
            var realms = AuthServer.Instance.Service.RealmStates.Select(r => r.Realm);

            var data = PacketHelper.Build(new S_RealmList
            {
                RealmCount = (byte)realms.Count(),
                Realms = realms.ToArray()
            });


            client.SendPacket(AuthOpcodes.RealmList, BitConverter.GetBytes((ushort)data.Length).Concat(data).ToArray());

            return true;
        }

        #endregion

    }
}
