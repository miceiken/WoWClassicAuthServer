using WoWClassic.Common.DataStructure;

namespace WoWClassic.Common.Protocol
{
    public class RealmState
    {
        public uint ID;
        public Realm Realm;
        public RealmStatus Status;
        public ushort GatewayPort;
    }

    public enum RealmStatus : byte
    {
        Online,
        Offline,
        Unavailable
    }
}
