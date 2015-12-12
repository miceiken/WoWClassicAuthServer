using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using WoWClassic.Cluster;
using WoWClassic.Common;
using WoWClassic.Common.Constants;
using WoWClassic.Common.Constants.Game;
using WoWClassic.Common.Crypto;
using WoWClassic.Common.Log;
using WoWClassic.Common.Packets;

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

        public ulong CharacterGUID { get; private set; }

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
            while ((bytesRead = m_Socket.Receive(m_RecvBuffer)) > 0)
            {
                var buffer = new byte[bytesRead];
                Buffer.BlockCopy(m_RecvBuffer, 0, buffer, 0, bytesRead);

                using (var ms = new MemoryStream(buffer))
                using (var br = new BinaryReader(ms))
                {
                    var header = new WorldPacketHeader(m_Crypt, br);

                    Log.WriteLine(GatewayLogTypes.Packets, $"<- {header.Opcode}({buffer.Length}):\n\t{string.Join(" ", buffer.Select(b => b.ToString("X2")))}");
                    if (!m_CommandHandlers.ContainsKey(header.Opcode) || !m_CommandHandlers[header.Opcode](br))
                    {
                        if (CharacterGUID == 0)
                            throw new Exception("Packet unhandled by Gateway -- Character GUID = 0");

                        Log.WriteLine(GatewayLogTypes.Packets, $"Forwarding {header.Opcode} to world server");
                        SendWorldPacket(header, buffer);
                    }
                }
            }
            m_Server.ClientConnections.Remove(this);
            Console.WriteLine("Dropped connection from {0}", ((IPEndPoint)m_Socket.RemoteEndPoint).Address);
        }

        private void SendWorldPacket(WorldPacketHeader header, byte[] data)
        {
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                bw.Write(CharacterGUID);
                // Replace header with the decrypted one
                Buffer.BlockCopy(header.GetDecrypted(), 0, data, 0, 6);
                bw.Write(data);

                m_Server.SendWorldPacket(this, ms.ToArray());
            }
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
                Log.WriteLine(GatewayLogTypes.Packets, $"-> {opcode}({packet.Length}):\n\t{string.Join(" ", packet.Select(b => b.ToString("X2")))}");
                m_Socket.Send(packet);
            }
        }

        public void SendPacket(byte[] data)
        {
            m_Crypt?.Encrypt(data);
            m_Socket.Send(data);
        }

        #endregion

        #region Packets

        #region SMSG_AUTH_CHALLENGE

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

        #endregion

        #region CMSG_PING

        public class CMSG_PING
        {
            public uint Ping;
            public uint Latency;
        }

        public class SMSG_PONG
        {
            public uint Ping;
        }

        [PacketHandler(WorldOpcodes.CMSG_PING)]
        public bool HandlePing(BinaryReader br)
        {
            var pkt = PacketHelper.Parse<CMSG_PING>(br);
            // TODO: implement ping checks

            SendPacket(WorldOpcodes.SMSG_PONG, PacketHelper.Build(new SMSG_PONG { Ping = pkt.Ping }));
            return true;
        }

        #endregion

        #region CMSG_AUTH_SESSION

        private class CMSG_AUTH_SESSION
        {
            public uint Build;
            private uint Unknown;
            [PacketString(StringTypes.CString)]
            public string Account;
            public uint ClientSeed;
            [PacketArrayLength(20)]
            public byte[] ClientDigest;
        }

        private class SMSG_AUTH_RESPONSE
        {
            public byte Response;
            public uint BillingTimeRemaining;
            public byte BillingPlanFlags;
            public uint BillingTimeRested;
        }

        [PacketHandler(WorldOpcodes.CMSG_AUTH_SESSION)]
        public bool HandleAuthSession(BinaryReader br)
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

        #region CMSG_CHAR_CREATE

        public class CMSG_CHAR_CREATE
        {
            [PacketString(StringTypes.CString)]
            public string Name;
            public WoWRace Race;
            public WoWClass Class;
            public WoWGender Gender;
            public byte Skin;
            public byte Face;
            public byte HairStyle;
            public byte HairColor;
            public byte FacialHair;
            public byte OutfitId;
        }

        public class SMSG_CHAR_CREATE
        {
            public CharacterCreationCode Code;
        }

        [PacketHandler(WorldOpcodes.CMSG_CHAR_CREATE)]
        public bool HandleCharCreate(BinaryReader br)
        {
            var pkt = PacketHelper.Parse<CMSG_CHAR_CREATE>(br);

            // TODO: character creation
            // for now, fake success!

            SendPacket(WorldOpcodes.SMSG_CHAR_CREATE, PacketHelper.Build(new SMSG_CHAR_CREATE { Code = CharacterCreationCode.Success }));
            return true;
        }

        #endregion

        #region CMSG_CHAR_ENUM

        private class CMSG_CHAR_ENUM
        {
        }

        private class SMSG_CHAR_ENUM
        {
            public byte Length;
            public CharEnumEntry[] Characters;
        }

        private class CharEnumEntry
        {
            public ulong GUID;
            [PacketString(StringTypes.CString)]
            public string Name;

            public WoWRace Race;
            public WoWClass Class;
            public WoWGender Gender;
            public byte Skin;
            public byte Face;
            public byte HairStyle;
            public byte HairColor;
            public byte FacialHair;

            public byte Level;
            public uint Zone;
            public uint Map;
            public float X, Y, Z;

            public uint GuildId;
            public uint CharacterFlags;
            public byte FirstLogin;

            public uint PetDisplayId;
            public uint PetLevel;
            public uint PetFamily;

            public CharEnumEquipmentEntry[] Equipment;
            public uint FirstBagDisplayId;
            public byte FirstBagInventoryType;
        }

        private class CharEnumEquipmentEntry
        {
            public uint DisplayInfoId;
            public byte InventoryType;
        }

        [PacketHandler(WorldOpcodes.CMSG_CHAR_ENUM)]
        public bool HandleCharEnum(BinaryReader br)
        {
            var pkt = PacketHelper.Parse<CMSG_CHAR_ENUM>(br);

            // TODO: Get characters from database

            var character = new CharEnumEntry
            {
                GUID = 1,
                Name = "ChaosvexIRL",
                Race = WoWRace.Human,
                Class = WoWClass.Paladin,
                Gender = WoWGender.Female,
                Skin = 1,
                Face = 7,
                HairStyle = 8,
                HairColor = 6,
                FacialHair = 4,

                Level = 60,
                Zone = 12,
                Map = 0,
                X = -8954.42f,
                Y = -158.558f,
                Z = 81.8225f,

                GuildId = 0,
                CharacterFlags = 0,
                FirstLogin = 0,

                PetDisplayId = 0,
                PetLevel = 0,
                PetFamily = 0,

                Equipment = new CharEnumEquipmentEntry[(int)WoWEquipSlot.Tabard + 1],
                FirstBagDisplayId = 0,
                FirstBagInventoryType = 0,
            };

            for (int i = (int)WoWEquipSlot.Head; i < (int)WoWEquipSlot.Tabard + 1; i++)
                character.Equipment[i] = new CharEnumEquipmentEntry { DisplayInfoId = 0, InventoryType = 0 };


            SendPacket(WorldOpcodes.SMSG_CHAR_ENUM, PacketHelper.Build(new SMSG_CHAR_ENUM
            {
                Length = 1,
                Characters = new CharEnumEntry[] { character },
            }));

            return true;
        }

        #endregion

        #region CMSG_PLAYER_LOGIN

        public class CMSG_PLAYER_LOGIN
        {
            public ulong GUID;
        }

        // https://github.com/cmangos/mangos-classic/blob/master/src/game/Player.cpp#L16858-L16925

        [PacketHandler(WorldOpcodes.CMSG_PLAYER_LOGIN)]
        public bool HandlePlayerLogin(BinaryReader br)
        {
            var pkt = PacketHelper.Parse<CMSG_PLAYER_LOGIN>(br);
            // TODO:
            // - Figure out where this character spawns
            // - Put it in the correct world
            CharacterGUID = pkt.GUID;

            return false; // Return false so world server also receives this packet, however, beforehand world server needs to know who it is
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