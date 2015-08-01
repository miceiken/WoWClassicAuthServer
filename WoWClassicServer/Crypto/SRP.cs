using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Security.Cryptography;
using System.Globalization;

namespace WoWClassicServer.Crypto
{
    public class SRP
    {
        public SRP(string I, string p)
        {
            s = m_Rng.Next(2 * 8) % N;
            x = H(s.ToString(), I, p);
            v = BigInteger.ModPow(g, x, N);

            // client->server (I)
            // server->client (s, B)
            b = m_Rng.Next(19 * 8) % N;
            B = (k * v + BigInteger.ModPow(g, b, N)) % N;
        }

        private Random m_Rng = new Random(Environment.TickCount);

        public BigInteger N { get; } = BigInteger.Parse("894B645E89E1535BBDAD5B8B290650530801B18EBFBF5E8FAB3C82872A3E9BB7", NumberStyles.HexNumber);
        public BigInteger g { get; } = 7;
        public BigInteger k { get; } = 3;
        public BigInteger s { get; private set; }
        public BigInteger x { get; private set; }
        public BigInteger v { get; private set; }
        public BigInteger b { get; private set; }
        public BigInteger B { get; private set; }
        public BigInteger A { get; set; }
        public BigInteger u { get { return H(A.ToString(), B.ToString()); } }
        public BigInteger SessionKey { get { return H(BigInteger.ModPow(A * BigInteger.ModPow(v, u, N), b, N).ToString()); } }
        public BigInteger M_c { get; set; }
        public BigInteger Proof { get { return H(A.ToString(), M_c.ToString(), SessionKey.ToString()); } }

        public BigInteger H(params string[] args)
        {
            return new BigInteger(Sha1Hash(string.Join(":", args)));
        }

        public static byte[] Sha1Hash(string s)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(s);
            var sha1 = SHA256.Create();
            return sha1.ComputeHash(bytes);
        }
    }
}
