using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using WoWClassic.Cluster;
using WoWClassic.Common.Constants;
using WoWClassic.Common.Constants.Game;
using WoWClassic.Common.Crypto;
using WoWClassic.Common.Packets;

namespace WoWClassic.Gateway
{
    public static class GatewayHandlers
    {
        static GatewayHandlers()
        {
            foreach (var method in typeof(GatewayHandlers).GetMethods(BindingFlags.Static | BindingFlags.Public))
            {
                var attr = method.GetCustomAttributes(typeof(PacketHandlerAttribute), false).Cast<PacketHandlerAttribute>().FirstOrDefault();
                if (attr == null) continue;
                PacketHandlers.Add((WorldOpcodes)attr.PacketId, (StaticCommandHandler<GatewayConnection>)method.CreateDelegate(typeof(StaticCommandHandler<GatewayConnection>), null));
            }
        }

        public static Dictionary<WorldOpcodes, StaticCommandHandler<GatewayConnection>> PacketHandlers
        {
            get; private set;
        } = new Dictionary<WorldOpcodes, StaticCommandHandler<GatewayConnection>>();

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
        public static bool HandlePing(GatewayConnection client, BinaryReader br)
        {
            var pkt = PacketHelper.Parse<CMSG_PING>(br);
            // TODO: implement ping checks

            client.SendPacket(WorldOpcodes.SMSG_PONG, PacketHelper.Build(new SMSG_PONG { Ping = pkt.Ping }));
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
        public static bool HandleAuthSession(GatewayConnection client, BinaryReader br)
        {
            var pkt = PacketHelper.Parse<CMSG_AUTH_SESSION>(br);

            // TODO: verify build

            client.Crypt = new AuthCrypt(LoginService.GetSessionKey(pkt.Account));

            var serverDigest = HashUtil.ComputeHash(Encoding.ASCII.GetBytes(pkt.Account),
                new byte[] { 0, 0, 0, 0 },
                BitConverter.GetBytes(pkt.ClientSeed),
                BitConverter.GetBytes(client.Seed),
                client.Crypt.SessionKey);
            if (!serverDigest.SequenceEqual(pkt.ClientDigest))
                return false;

            client.SendPacket(WorldOpcodes.SMSG_AUTH_RESPONSE, PacketHelper.Build(new SMSG_AUTH_RESPONSE
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
        public static bool HandleCharCreate(GatewayConnection client, BinaryReader br)
        {
            var pkt = PacketHelper.Parse<CMSG_CHAR_CREATE>(br);

            // TODO: character creation
            // for now, fake success!

            client.SendPacket(WorldOpcodes.SMSG_CHAR_CREATE, PacketHelper.Build(new SMSG_CHAR_CREATE { Code = CharacterCreationCode.Success }));
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
        public static bool HandleCharEnum(GatewayConnection client, BinaryReader br)
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


            client.SendPacket(WorldOpcodes.SMSG_CHAR_ENUM, PacketHelper.Build(new SMSG_CHAR_ENUM
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
        public static bool HandlePlayerLogin(GatewayConnection client, BinaryReader br)
        {
            var pkt = PacketHelper.Parse<CMSG_PLAYER_LOGIN>(br);
            // TODO:
            // - Figure out where this character spawns
            // - Put it in the correct world
            client.CharacterGUID = pkt.GUID;
            // Map guid to object for easy lookup
            client.GatewaySrv.WorldGatewayServer.GUIDClientMap.Add(client.CharacterGUID, client);

            return false; // Return false so world server also receives this packet, however, beforehand world server needs to know who it is
        }

        #endregion
    }
}
