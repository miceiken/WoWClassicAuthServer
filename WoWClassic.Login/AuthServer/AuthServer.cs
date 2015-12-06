using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using WoWClassic.Common.Constants;
using WoWClassic.Cluster;

namespace WoWClassic.Login.AuthServer
{
    public class AuthServer
    {
        public AuthServer()
        {
            m_Listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        private Socket m_Listener;
        private Thread m_ThreadAcceptConnections;

        public LoginService Service { get; private set; } = new LoginService();

        public List<AuthConnection> Clients { get; private set; } = new List<AuthConnection>();

        public void Listen(IPEndPoint endpoint)
        {
            m_Listener.Bind(endpoint);
            m_Listener.Listen(6600);

            Service.Participate();

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
