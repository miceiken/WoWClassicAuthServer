using LinqToDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using WoWClassic.Common;
using WoWClassic.Common.Crypto;
using WoWClassic.Common.Packets;
using WoWClassic.Common.Protocol;
using WoWClassic.Datastore.Login;
using WoWClassic.Common.DataStructure;

namespace WoWClassic.Cluster
{
    public class LoginService : ClusterService<LoginServicePacketIds>
    {
        public LoginService()
            : base(ServiceIds.Login)
        { }

        public List<RealmState> RealmStates { get; private set; } = new List<RealmState>();

        public static void CreateAccount(string username, string email, string password)
        {
            username = username.ToUpper();
            password = password.ToUpper();
            using (var db = new DBLogin())
            using (var sha = SHA1.Create())
            {
                var srp = new SRP(username, password);
                var statement = db.Account
                    .Value(a => a.Username, username)
                    .Value(a => a.PasswordHash, sha.ComputeHash(Encoding.ASCII.GetBytes(password)))
                    .Value(a => a.Email, email)
                    .Value(a => a.SRPVerifier, srp.Verifier.ToProperByteArray())
                    .Value(a => a.SRPSalt, srp.Salt.ToProperByteArray());
                statement.Insert();
            }
        }

        public static bool ExistsAccount(string username)
        {
            using (var db = new DBLogin())
            {
                return db.Account.FirstOrDefault(a => a.Username == username.ToUpper()) != null;
            }
        }

        public static SRP GetAccountSecurity(string username)
        {
            using (var db = new DBLogin())
            {
                var acc = db.Account.FirstOrDefault(a => a.Username == username);
                return new SRP(acc.Username, acc.SRPSalt.ToPositiveBigInteger(), acc.SRPVerifier.ToPositiveBigInteger());
            }
        }

        public static byte[] GetSessionKey(string username)
        {
            using (var db = new DBLogin())
                return db.Account.FirstOrDefault(a => a.Username == username).SessionKey;
        }

        public static void UpdateSessionKey(string username, byte[] key)
        {
            using (var db = new DBLogin())
                db.Account.Where(a => a.Username == username)
                    .Set(a => a.SessionKey, key)
                    .Update();
        }

        [PacketHandler(GatewayServicePacketIds.RealmState)]
        public bool HandleRealmUpdate(BinaryReader br)
        {
            var realm = PacketHelper.Parse<RealmState>(br);

            if (RealmStates.Exists(r => r.ID == realm.ID))
                RealmStates.Remove(RealmStates.FirstOrDefault(r => r.ID == realm.ID));

            Console.WriteLine($"CLUSTER: Received realm #{realm.ID} '{realm.Realm.Name}' from gateway");
            RealmStates.Add(realm);

            return true;
        }

    }
}
