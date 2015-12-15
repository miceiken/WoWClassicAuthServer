using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using WoWClassic.Common.Constants;
using WoWClassic.Common.Packets;

namespace WoWClassic.World
{
    public static class WorldHandler
    {
        static WorldHandler()
        {
            foreach (var method in typeof(WorldHandler).GetMethods(BindingFlags.Static | BindingFlags.Public))
            {
                var attr = method.GetCustomAttributes(typeof(PacketHandlerAttribute), false).Cast<PacketHandlerAttribute>().FirstOrDefault();
                if (attr == null) continue;
                PacketHandlers.Add((WorldOpcodes)attr.PacketId, (StaticCommandHandler<WorldClient>)method.CreateDelegate(typeof(StaticCommandHandler<WorldClient>), null));
            }
        }

        public static Dictionary<WorldOpcodes, StaticCommandHandler<WorldClient>> PacketHandlers
        {
            get; private set;
        } = new Dictionary<WorldOpcodes, StaticCommandHandler<WorldClient>>();

        #region CMSG_PLAYER_LOGIN

        // https://github.com/cmangos/mangos-classic/blob/master/src/game/CharacterHandler.cpp#L417-L669


        public class CMSG_PLAYER_LOGIN
        {
            public ulong GUID;
        }

        public class SMSG_LOGIN_VERIFY_WORLD
        {
            public uint MapID;
            public float X, Y, Z;
            public float Orientation;
        }

        public class SMSG_ACCOUNT_DATA_TIMES
        {
            public uint[] Data;
        }

        // https://github.com/cmangos/mangos-classic/blob/master/src/game/Player.cpp#L16858-L16925

        [PacketHandler(WorldOpcodes.CMSG_PLAYER_LOGIN)]
        public static bool HandlePlayerLogin(WorldClient client, BinaryReader br)
        {
            var pkt = PacketHelper.Parse<CMSG_PLAYER_LOGIN>(br);

            client.SendPacket(WorldOpcodes.SMSG_LOGIN_VERIFY_WORLD, PacketHelper.Build(new SMSG_LOGIN_VERIFY_WORLD
            {
                MapID = 0,
                X = -8954.42f,
                Y = -158.558f,
                Z = 81.8225f,
                Orientation = 0.0f
            }));

            client.SendPacket(WorldOpcodes.SMSG_ACCOUNT_DATA_TIMES, PacketHelper.Build(new SMSG_ACCOUNT_DATA_TIMES { Data = new uint[32] }));
            return true;
        }

        #endregion
    }
}
