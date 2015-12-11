using System;
using LinqToDB;
using LinqToDB.Data;

namespace WoWClassic.Datastore.Login
{
    public class DBLogin : DataConnection
    {
        public DBLogin() : base("Login") { }

        public ITable<Account> Account { get { return GetTable<Account>(); } }
        public ITable<Session> Session { get { return GetTable<Session>(); } }
        public ITable<Realm> Realm { get { return GetTable<Realm>(); } }
    }
}
