using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWClassic.Common.Protocol;
using WoWClassic.Common;
using WoWClassic.Common.Constants;
using System.IO;

namespace WoWClassic.Cluster
{
    public class LoginService : ClusterService<LoginServicePacketIds>
    {
        public LoginService()
            : base(ServiceIds.Login)
        { }

        public List<RealmInfo> Realms { get; private set; } = new List<RealmInfo>();

        [PacketHandler(GatewayServicePacketIds.UpdateRealm)]
        public bool HandleRealmUpdate(BinaryReader br, int bytesRead)
        {
            var realm = RealmInfo.Read(br);

            var existing = Realms.FirstOrDefault(r => r.Name == realm.Name);
            if (!string.IsNullOrEmpty(existing.Name))
                Realms.Remove(existing);

            Console.WriteLine("CLUSTER: Received realm '{0}' from Gateway", realm.Name);
            Realms.Add(realm);

            return true;
        }

    }
}
