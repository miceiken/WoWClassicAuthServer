using LinqToDB;
using System;
using System.IO;
using WoWClassic.Cluster;
using WoWClassic.Common.Network;
using WoWClassic.Datastore.Login;

namespace WoWClassic.Login
{
    public class AuthServer : Server
    {
        public static AuthServer Instance = new AuthServer();

        public AuthServer()
        {
            Service = new LoginService();
            Service.Participate();

            InitTestDb();
        }

        public LoginService Service { get; private set; }

        protected override void AcceptCallback(IAsyncResult ar)
        {
            Connections.Add(new AuthConnection(this, m_Listener.EndAccept(ar)));
            base.AcceptCallback(ar);
        }

        private void InitTestDb()
        {
            if (!File.Exists("Login.sqlite"))
                using (var db = new DBLogin())
                    db.CreateTable<Account>();

            if (!LoginService.ExistsAccount("testuser"))
                LoginService.CreateAccount("testuser", "test@test.com", "TestPass");
        }
    }
}
