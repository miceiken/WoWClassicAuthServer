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

        [Column/*(Length = 40)*/, NotNull]
        public byte[] PasswordHash { get; set; }

        [Column/*(Length = 40)*/]
        public byte[] SessionKey { get; set; }

        [Column/*(Length = 32)*/, NotNull]
        public byte[] SRPVerifier { get; set; }

        [Column/*(Length = 32)*/, NotNull]
        public byte[] SRPSalt { get; set; }
    }
}
