using System;
using LinqToDB;
using LinqToDB.Data;

namespace WoWClassic.Datastore.Gateway
{
    public class DBGateway : DataConnection
    {
        public DBGateway() : base("Gateway") { }

        public ITable<Character> Character { get { return GetTable<Character>(); } }
    }
}
