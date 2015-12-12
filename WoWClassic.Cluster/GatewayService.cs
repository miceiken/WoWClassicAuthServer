using WoWClassic.Common.Constants;
using WoWClassic.Common.Packets;
using WoWClassic.Common.Protocol;
using WoWClassic.Common.DataStructure;

namespace WoWClassic.Cluster
{
    public class GatewayService : ClusterService<GatewayServicePacketIds>
    {
        public GatewayService()
            : base(ServiceIds.Gateway)
        { }

        private RealmState m_RealmState;
        public RealmState RealmState
        {
            get { return m_RealmState; }
            set
            {
                m_RealmState = value;
                Announce(GatewayServicePacketIds.RealmState, PacketHelper.Build(m_RealmState));
            }
        }
    }
}
