using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using WoWClassic.Cluster;
using WoWClassic.Common;
using WoWClassic.Common.Constants;
using WoWClassic.Common.Constants.Game;
using WoWClassic.Common.Crypto;
using WoWClassic.Common.Packets;
using LinqToDB;
using WoWClassic.Datastore.Gateway;
using WoWClassic.Datastore;
using WoWClassic.Datastore.Login;

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

            // TODO: Move to LoginService?
            using (var db = new DBLogin())
            {
                var acc = db.Account.FirstOrDefault(a => a.Username == pkt.Account);
                client.AccountName = acc.Username;
                client.AccountID = acc.AccountID;
            }

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
            pkt.Name = pkt.Name.UppercaseFirst();

            using (var db = new DBGateway())
            {
                // TODO: If PvP-server, make sure same-faction
                // TODO: Check for reserved names

                // Check if name already is taken
                if (db.Character.Count(c => c.Name == pkt.Name) > 0)
                {
                    client.SendPacket(WorldOpcodes.SMSG_CHAR_CREATE, PacketHelper.Build(new SMSG_CHAR_CREATE { Code = CharacterCreationCode.NameInUse }));
                    return true;
                }

                db.Insert(new Character
                {
                    AccountID = client.AccountID,

                    Name = pkt.Name,
                    Race = (byte)pkt.Race,
                    Class = (byte)pkt.Class,
                    Gender = (byte)pkt.Gender,
                    Skin = pkt.Skin,
                    Face = pkt.Face,
                    HairStyle = pkt.HairStyle,
                    HairColor = pkt.HairColor,
                    FacialHair = pkt.FacialHair,

                    Level = 1,
                    // TODO: Get spawn information based on race
                    Zone = 12,
                    Map = 0,
                    X = -8954.42f,
                    Y = -158.558f,
                    Z = 81.8225f,

                    GuildId = 0, // New characters don't have guilds!
                    CharacterFlags = 0,
                    FirstLogin = false, //TODO: set true
                });
            }

            client.SendPacket(WorldOpcodes.SMSG_CHAR_CREATE, PacketHelper.Build(new SMSG_CHAR_CREATE { Code = CharacterCreationCode.Success }));
            return true;
        }

        #endregion

        #region CMSG_CHAR_DELETE

        public class CMSG_CHAR_DELETE
        {
            public ulong GUID;
        }

        public class SMSG_CHAR_DELETE
        {
            public CharacterDeleteCode Code;
        }

        [PacketHandler(WorldOpcodes.CMSG_CHAR_DELETE)]
        public static bool HandleCharDelete(GatewayConnection client, BinaryReader br)
        {
            var pkt = PacketHelper.Parse<CMSG_CHAR_DELETE>(br);

            using (var db = new DBGateway())
            {
                db.Character.Delete(c => c.CharacterID == pkt.GUID);
            }

            client.SendPacket(WorldOpcodes.SMSG_CHAR_DELETE, PacketHelper.Build(new SMSG_CHAR_DELETE { Code = CharacterDeleteCode.Success }));

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

            // TODO: Move to gateway service?

            using (var db = new DBGateway())
            {
                var dbCharacters = db.Character.Where(c => c.AccountID == client.AccountID);
                if (dbCharacters.Count() == 0)
                {
                    client.SendPacket(WorldOpcodes.SMSG_CHAR_ENUM, PacketHelper.Build(new SMSG_CHAR_ENUM { Length = 0 }));
                    return true;
                }

                var characters = new List<CharEnumEntry>();
                foreach (var character in dbCharacters)
                {
                    var gameChar = new CharEnumEntry
                    {
                        GUID = character.CharacterID,

                        Name = character.Name,
                        Race = (WoWRace)character.Race,
                        Class = (WoWClass)character.Class,
                        Gender = (WoWGender)character.Gender,
                        Skin = character.Skin,
                        Face = character.Face,
                        HairStyle = character.HairStyle,
                        HairColor = character.HairColor,
                        FacialHair = character.FacialHair,

                        Level = character.Level,
                        Zone = character.Zone,
                        Map = character.Zone,
                        X = character.X,
                        Y = character.Y,
                        Z = character.Z,

                        GuildId = character.GuildId,
                        CharacterFlags = character.CharacterFlags,
                        FirstLogin = (byte)(character.FirstLogin ? 1 : 0),

                        // TODO: Get rest from database
                        PetDisplayId = 0,
                        PetLevel = 0,
                        PetFamily = 0,

                        Equipment = new CharEnumEquipmentEntry[(int)WoWEquipSlot.Tabard + 1],
                        FirstBagDisplayId = 0,
                        FirstBagInventoryType = 0,
                    };

                    for (int i = (int)WoWEquipSlot.Head; i < (int)WoWEquipSlot.Tabard + 1; i++)
                        gameChar.Equipment[i] = new CharEnumEquipmentEntry { DisplayInfoId = 0, InventoryType = 0 };

                    characters.Add(gameChar);
                }

                client.SendPacket(WorldOpcodes.SMSG_CHAR_ENUM, PacketHelper.Build(new SMSG_CHAR_ENUM
                {
                    Length = (byte)characters.Count,
                    Characters = characters.ToArray(),
                }));
            }

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
