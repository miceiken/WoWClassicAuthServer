using System;
using System.IO;
using WoWClassic.Common.Constants;
using WoWClassic.Common.Protocol;

namespace WoWClassic.Common.Packets
{
    public delegate bool CommandHandler(BinaryReader br);
    public delegate bool StaticCommandHandler<T>(T client, BinaryReader br);

    [AttributeUsage(AttributeTargets.Method)]
    public class PacketHandlerAttribute : Attribute
    {

        public PacketHandlerAttribute(int packetId, Type pType)
        {
            PacketId = packetId;
            PacketType = pType;
        }

        public PacketHandlerAttribute(AuthOpcodes packetId)
            : this(Convert.ToInt32(packetId), typeof(AuthOpcodes))
        { }

        public PacketHandlerAttribute(WorldOpcodes packetId)
            : this(Convert.ToInt32(packetId), typeof(WorldOpcodes))
        { }

        #region Cluster Protocol

        public PacketHandlerAttribute(GatewayServicePacketIds packetId)
            : this(Convert.ToInt32(packetId), typeof(GatewayServicePacketIds))
        { }

        public PacketHandlerAttribute(LoginServicePacketIds packetId)
            : this(Convert.ToInt32(packetId), typeof(LoginServicePacketIds))
        { }

        public PacketHandlerAttribute(WorldServicePacketIds packetId)
            : this(Convert.ToInt32(packetId), typeof(WorldServicePacketIds))
        { }

        public PacketHandlerAttribute(DatabaseServicePacketIds packetId)
            : this(Convert.ToInt32(packetId), typeof(DatabaseServicePacketIds))
        { }

        #endregion

        public int PacketId { get; private set; }
        public Type PacketType { get; private set; }
    }
}
