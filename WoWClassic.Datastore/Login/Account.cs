using System;
using LinqToDB.Mapping;

namespace WoWClassic.Datastore.Login
{
    [Table(Name = "Accounts")]
    public class Account
    {
        [PrimaryKey, Identity]
        public int AccountID { get; set; }

        [Column(Length = 32), NotNull]
        public string Username { get; set; }

        [Column(Length = 32), NotNull]
        public string Email { get; set; }

        [Column, NotNull]
        public byte[] PasswordHash { get; set; }

        [Column, NotNull]
        public byte[] SRPVerifier { get; set; }

        [Column, NotNull]
        public byte[] SRPSalt { get; set; }

        [Association(ThisKey = "AccountID", OtherKey = "AccountID", CanBeNull = false)]
        public Session Session { get; set; }
    }
}
