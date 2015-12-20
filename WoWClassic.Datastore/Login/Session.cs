using System;
using LinqToDB.Mapping;

namespace WoWClassic.Datastore.Login
{
    [Table(Name = "Sessions")]
    public class Session
    {
        [PrimaryKey, Identity]
        public int AccountID { get; set; }

        [Column]
        public byte[] SessionKey { get; set; }

        [Association(ThisKey = "AccountID", OtherKey = "AccountID", CanBeNull = false)]
        public Account Account { get; set; }
    }
}
