using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using WoWClassic.Common;
using WoWClassic.Common.Crypto;
using WoWClassic.Common.Packets;
using WoWClassic.Common.Protocol;
using WoWClassic.Datastore.Login;
using WoWClassic.Common.DataStructure;

namespace WoWClassic.Cluster
{
    public class WorldService : ClusterService<WorldServicePacketIds>
    {
        public WorldService()
            : base(ServiceIds.World)
        { }

        public uint RealmID { get; set; }
        public RealmState RealmState { get; set; }

        public EventHandler RealmStatusChanged;

        [PacketHandler(GatewayServicePacketIds.RealmState)]
        public bool HandleRealmUpdate(BinaryReader br)
        {
            var realm = PacketHelper.Parse<RealmState>(br);

            if (realm.ID != RealmID)
                return true;

            RealmState = realm;

            if (RealmStatusChanged != null)
                RealmStatusChanged(this, new EventArgs());

            return true;
        }
    }
}
