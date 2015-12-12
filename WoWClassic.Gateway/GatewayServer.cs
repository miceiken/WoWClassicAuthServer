using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using WoWClassic.Cluster;
using WoWClassic.Common.DataStructure;
using WoWClassic.Common.Protocol;
using System.Linq;
using System;

namespace WoWClassic.Gateway
{
    public class GatewayServer
    {
        public GatewayServer()
        {
            m_Listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        private Socket m_Listener;
        private Thread m_ThreadAcceptConnections;

        public GatewayService Service { get; private set; } = new GatewayService();
        public WorldGatewayServer WorldGatewayServer { get; private set; }

        public List<GatewayConnection> ClientConnections { get; private set; } = new List<GatewayConnection>();

        public void Listen(IPEndPoint endpoint)
        {
            m_Listener.Bind(endpoint);
            m_Listener.Listen(6600);

            // Initialize WorldGatewayServer, so world servers can connect
            WorldGatewayServer = new WorldGatewayServer(this);
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

            m_ThreadAcceptConnections = new Thread(OnAccept);
            m_ThreadAcceptConnections.Start();
        }

        private void OnAccept()
        {
            Socket accepted;
            while ((accepted = m_Listener.Accept()) != null)
            {
                accepted.LingerState = new LingerOption(true, 5);
                var client = new GatewayConnection(accepted, this);

                lock (ClientConnections)
                    ClientConnections.Add(client);
            }
        }

        public void SendWorldPacket(GatewayConnection connection, byte[] data)
        {
            WorldGatewayConnection worldServer;
            if (!WorldGatewayServer.ClientConnectionMap.TryGetValue(connection, out worldServer))
            {
                if (WorldGatewayServer.Connections.Count == 0)
                    throw new Exception("We have client connections, but no world servers");
                worldServer = WorldGatewayServer.Connections.FirstOrDefault();
                if (worldServer == null)
                    throw new Exception("Can't find suitable world server for client");
                WorldGatewayServer.ClientConnectionMap[connection] = worldServer;
            }
            worldServer.SendPacket(data);
        }
    }
}
