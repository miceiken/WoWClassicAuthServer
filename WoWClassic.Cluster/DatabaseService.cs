using WoWClassic.Common.Protocol;

namespace WoWClassic.Cluster
{
    public class DatabaseService : ClusterService<DatabaseServicePacketIds>
    {
        public DatabaseService()
            : base(ServiceIds.Database)
        { }
    }
}
