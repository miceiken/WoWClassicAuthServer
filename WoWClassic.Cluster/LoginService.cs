using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWClassic.Common.Protocol;
using WoWClassic.Common;
using WoWClassic.Common.Constants;
using System.IO;
using LinqToDB;
using LinqToDB.Mapping;
using WoWClassic.Datastore;
using WoWClassic.Datastore.Login;
using System.Security.Cryptography;
using WoWClassic.Common.Crypto;
using System.Numerics;

namespace WoWClassic.Cluster
{
    public class LoginService : ClusterService<LoginServicePacketIds>
    {
        public LoginService()
            : base(ServiceIds.Login)
        { }

        public List<RealmInfo> Realms { get; private set; } = new List<RealmInfo>();

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

        [PacketHandler(GatewayServicePacketIds.UpdateRealm)]
        public bool HandleRealmUpdate(BinaryReader br, int bytesRead)
        {
            var realm = RealmInfo.Read(br);

            if (Realms.Contains(realm))
                Realms.Remove(realm);

            Console.WriteLine("CLUSTER: Received realm '{0}' from gateway", realm.Name);
            Realms.Add(realm);

            return true;
        }

    }
}
