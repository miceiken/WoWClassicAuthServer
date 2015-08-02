using System;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace WoWClassicServer.Crypto
{
    public class SRP
    {
        /*
         * SRP - Secure Remote Password
         *  implemented by miceiken
         * Heavily inspired by https://en.wikipedia.org/wiki/Secure_Remote_Password_protocol#Implementation_example_in_Python
         */

        public SRP(string I, string p)
        {
            s = m_Rng.Next(2 * 8) % N;
            x = H(BytesToString(s.ToByteArray()), I, p);
            v = BigInteger.ModPow(g, x, N);

            b = m_Rng.Next(19 * 8) % N;
            B = (k * v + BigInteger.ModPow(g, b, N)) % N;
        }

        private Random m_Rng = new Random(Environment.TickCount);

        public BigInteger N { get; } = BigInteger.Parse("0894B645E89E1535BBDAD5B8B290650530801B18EBFBF5E8FAB3C82872A3E9BB7", NumberStyles.HexNumber);
        public BigInteger g { get; } = 7;
        public BigInteger k { get; } = 3;
        public BigInteger s { get; private set; }
        public BigInteger x { get; private set; }
        public BigInteger v { get; private set; }
        public BigInteger b { get; private set; }
        public BigInteger B { get; private set; }

        public BigInteger A { get; set; }
        public BigInteger u { get { return H(A.ToByteArray(), B.ToByteArray()); } }
        public BigInteger K_s { get { return H(BigInteger.ModPow(A * BigInteger.ModPow(v, u, N), b, N).ToByteArray()); } } // SessionKey
        public BigInteger M_c { get; set; }
        public BigInteger M_s { get { return H(A.ToByteArray(), M_c.ToByteArray(), K_s.ToByteArray()); } } // Proof

        public void SetBinary(out BigInteger field, byte[] val)
        {
            //Array.Reverse(val);
            field = new BigInteger(AppendBytes(val));
        }

        public BigInteger H(params string[] args)
        {
            return new BigInteger(AppendBytes(Sha1Hash(string.Join(":", args))));
        }

        public BigInteger H(params byte[][] args)
        {
            return new BigInteger(AppendBytes(Sha1Hash(string.Join(":", args.Select(b => BytesToString(b))))));
        }

        private static string BytesToString(byte[] bytes)
        {
            return string.Join("", bytes.Select(b => b.ToString("X2")));
        }

        public static byte[] Sha1Hash(string s)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(s);
            var sha1 = SHA1.Create();
            return sha1.ComputeHash(bytes);
        }

        public static byte[] Sha1Hash(byte[] bytes)
        {
            var sha1 = SHA1.Create();
            return sha1.ComputeHash(bytes);
        }

        // http://stackoverflow.com/a/5649264
        private static byte[] AppendBytes(byte[] bytes)
        {
            return bytes.Concat(new byte[] { 0 }).ToArray();
        }
    }
}
