using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWClassic.Common.Protocol;
using WoWClassic.Common;
using WoWClassic.Common.Constants;

namespace WoWClassic.Cluster
{
    public class GatewayService : ClusterService<GatewayServicePacketIds>
    {
        public GatewayService()
            : base(ServiceIds.Gateway)
        { }

        private RealmInfo m_Realm;
        public RealmInfo Realm
        {
            get { return m_Realm; }
            set
            {
                m_Realm = value;
                Announce(GatewayServicePacketIds.UpdateRealm, m_Realm.ToByteArray());
            }
        }


    }
}
