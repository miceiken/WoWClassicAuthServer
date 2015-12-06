using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using WoWClassic.Common.Constants;
using WoWClassic.Cluster;
using WoWClassic.Datastore;
using WoWClassic.Datastore.Login;
using LinqToDB;
using LinqToDB.Mapping;
using System.IO;

namespace WoWClassic.Login.AuthServer
{
    public class AuthServer
    {
        public AuthServer()
        {
            m_Listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            InitTestDb();
        }

        private Socket m_Listener;
        private Thread m_ThreadAcceptConnections;

        public LoginService Service { get; private set; } = new LoginService();

        public List<AuthConnection> Clients { get; private set; } = new List<AuthConnection>();

        private void InitTestDb()
        {
            if (!File.Exists("Login.sqlite"))
                using (var db = new DBLogin())
                    db.CreateTable<Account>();
            if (!LoginService.ExistsAccount("testuser"))
                LoginService.CreateAccount("testuser", "test@test.com", "TestPass");

            if (!LoginService.ExistsAccount("miceiken"))
                LoginService.CreateAccount("miceiken", "test@test.com", "miceiken");
        }

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
