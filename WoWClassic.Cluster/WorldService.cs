using WoWClassic.Common.Protocol;

namespace WoWClassic.Cluster
{
    public class WorldService : ClusterService<WorldServicePacketIds>
    {
        public WorldService()
            : base(ServiceIds.World)
        { }
    }
}
