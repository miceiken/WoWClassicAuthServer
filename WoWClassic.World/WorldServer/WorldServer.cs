using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace WoWClassic.WorldServer.WorldServer
{
    public class WorldServer
    {
        public WorldServer()
        {
            m_Listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Clients = new List<AuthConnection>();
        }

        private readonly Socket m_Listener;
        private Thread m_ThreadAcceptConnections;

        public List<AuthConnection> Clients { get; private set; }
        public List<RealmInfo> Realms { get; private set; }

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
                var client = new AuthConnection(accepted, this);

                lock (Clients)
                    Clients.Add(client);
            }
        }
    }
}
