using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWClassic.Common;
using WoWClassic.Common.Packets;
using WoWClassic.Common.Constants;
using System.Reflection;
using System.IO;

namespace WoWClassic.World
{
    public static class WorldHandler
    {
        static WorldHandler()
        {
            PacketHandlers = RegisterHandlers();
        }

        public static readonly Dictionary<WorldOpcodes, StaticCommandHandler<Client>> PacketHandlers;

        #region Packet Handler Reflection

        private static Dictionary<WorldOpcodes, StaticCommandHandler<Client>> RegisterHandlers()
        {
            var ret = new Dictionary<WorldOpcodes, StaticCommandHandler<Client>>();
            var type = typeof(WorldHandler);
            foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public))
            {
                var attr = method.GetCustomAttributes(typeof(PacketHandlerAttribute), false).Cast<PacketHandlerAttribute>().FirstOrDefault();
                if (attr == null) continue;
                ret.Add((WorldOpcodes)attr.PacketId, (StaticCommandHandler<Client>)method.CreateDelegate(typeof(StaticCommandHandler<Client>), null));
            }
            return ret;
        }

        #endregion

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
        public static bool HandlePlayerLogin(Client client, BinaryReader br)
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
