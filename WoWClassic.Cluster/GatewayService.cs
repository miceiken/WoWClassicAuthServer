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

        private Realm m_Realm;
        public Realm Realm
        {
            get { return m_Realm; }
            set
            {
                m_Realm = value;
                Announce(GatewayServicePacketIds.UpdateRealm, PacketHelper.Build(m_Realm));
            }
        }


    }
}
