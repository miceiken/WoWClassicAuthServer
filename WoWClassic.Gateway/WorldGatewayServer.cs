using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using WoWClassic.Cluster;
using WoWClassic.Common.DataStructure;

namespace WoWClassic.Gateway
{
    public class WorldGatewayServer
    {
        public WorldGatewayServer(GatewayServer server)
        {
            m_Listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        private Socket m_Listener;
        private Thread m_ThreadAcceptConnections;

        public List<WorldGatewayConnection> Connections { get; private set; } = new List<WorldGatewayConnection>();
        public Dictionary<ulong, GatewayConnection> GUIDClientMap { get; private set; } = new Dictionary<ulong, GatewayConnection>();
        public Dictionary<GatewayConnection, WorldGatewayConnection> ClientConnectionMap { get; private set; } = new Dictionary<GatewayConnection, WorldGatewayConnection>();

        public void Listen(IPEndPoint endpoint)
        {
            m_Listener.Bind(endpoint);
            m_Listener.Listen(6600);

            m_ThreadAcceptConnections = new Thread(OnAccept);
            m_ThreadAcceptConnections.Start();
        }

        private void OnAccept()
        {
            Socket accepted;
            while ((accepted = m_Listener.Accept()) != null)
            {
                accepted.LingerState = new LingerOption(true, 5);
                var client = new WorldGatewayConnection(accepted, this);

                lock (Connections)
                    Connections.Add(client);
            }
        }
    }
}
