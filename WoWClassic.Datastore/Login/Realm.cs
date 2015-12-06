using System;
using System.Net;
using LinqToDB.Mapping;

namespace WoWClassic.Datastore.Login
{
    [Table(Name = "Realms")]
    public class Realm
    {
        [PrimaryKey, Identity]
        public int RealmID { get; set; }

        [Column, NotNull]
        public string Name { get; set; }

        [Column, NotNull]
        public IPAddress Address { get; set; }

        [Column, NotNull]
        public ushort Port { get; set; }

        [Column, NotNull]
        public uint Type { get; set; }

        [Column, NotNull]
        public byte Flags { get; set; }

        [Column, NotNull]
        public byte Timezone { get; set; }

        [Column, NotNull]
        public float Population { get; set; }
    }
}
