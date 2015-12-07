using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using WoWClassic.Cluster;
using WoWClassic.Common;
using WoWClassic.Common.Constants;

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

        public List<GatewayConnection> Clients { get; private set; } = new List<GatewayConnection>();

        public void Listen(IPEndPoint endpoint)
        {
            m_Listener.Bind(endpoint);
            m_Listener.Listen(6600);

            Service.Participate();
            Service.Realm = new RealmInfo()
            {
                Type = RealmType.Normal,
                Flags = RealmFlags.None,
                Name = "Test Server",
                Address = "127.0.0.1:8085", // 8085
                Population = RealmPopulationPreset.Low,
                Characters = 0,
                Timezone = RealmTimezone.AnyLocale
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

                lock (Clients)
                    Clients.Add(client);
            }
        }
    }
}
