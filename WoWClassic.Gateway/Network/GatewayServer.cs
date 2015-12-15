using System;
using System.Linq;
using System.Net;
using WoWClassic.Cluster;
using WoWClassic.Common.DataStructure;
using WoWClassic.Common.Network;
using WoWClassic.Common.Protocol;

namespace WoWClassic.Gateway
{
    public class GatewayServer : Server
    {
        public GatewayServer()
        { }

        protected override void AcceptCallback(IAsyncResult ar)
        {
            Connections.Add(new GatewayConnection(this, m_Listener.EndAccept(ar)));
            base.AcceptCallback(ar);
        }

        public GatewayService Service { get; private set; } = new GatewayService();
        public WorldGatewayServer WorldGatewayServer { get; private set; }

        public override void Listen(IPEndPoint endPoint)
        {
            base.Listen(endPoint);

            // Initialize WorldGatewayServer, so world servers can connect
            WorldGatewayServer = new WorldGatewayServer();
            WorldGatewayServer.Listen(new IPEndPoint(IPAddress.Any, 8090));

            // Announce the realm (triggering world servers to connect)
            Service.Participate();
            // TODO: Load realm from database / config
            Service.RealmState = new RealmState
            {
                ID = 1,
                Realm = new Realm()
                {
                    Type = RealmType.Normal,
                    Flags = RealmFlags.None,
                    Name = "Test Server",
                    Address = "127.0.0.1:8085",
                    Population = RealmPopulationPreset.Low,
                    Characters = 0,
                    Timezone = RealmTimezone.AnyLocale,
                },
                Status = RealmStatus.Online,
                GatewayPort = 8090
            };
        }

        public void SendWorldPacket(GatewayConnection connection, byte[] data)
        {
            WorldGatewayConnection worldServer;
            if (!WorldGatewayServer.ClientConnectionMap.TryGetValue(connection, out worldServer))
            {
                if (WorldGatewayServer.Connections.Count == 0)
                    throw new Exception("We have client connections, but no world servers");
                worldServer = WorldGatewayServer.Connections.Cast<WorldGatewayConnection>().FirstOrDefault();
                if (worldServer == null)
                    throw new Exception("Can't find suitable world server for client");
                WorldGatewayServer.ClientConnectionMap[connection] = worldServer;
            }
            worldServer.Send(data);
        }
    }
}
