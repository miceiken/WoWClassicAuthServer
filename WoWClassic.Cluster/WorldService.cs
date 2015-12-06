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
    public class WorldService : ClusterService<WorldServicePacketIds>
    {
        public WorldService()
            : base(ServiceIds.World)
        { }
    }
}
