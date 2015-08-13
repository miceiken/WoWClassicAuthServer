using System;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace WoWClassicServer.Crypto
{
    public class SRP // TODO: Clean up and make decent names
    {
        // From password
        public SRP(string I, string p)
        {
            this.I = I;
            s = GetRandomNumber(32) % N;
            x = H(s.ToProperByteArray(), H(Encoding.ASCII.GetBytes(I + ":" + p)).ToProperByteArray());
            v = BigInteger.ModPow(g, x, N); ;

            b = GetRandomNumber(19) % N;
            B = (k * v + BigInteger.ModPow(g, b, N)) % N;
        }

        // From s and v
        public SRP(BigInteger s, BigInteger v)
        {
            this.s = s;
            this.v = v;
        }

        private RNGCryptoServiceProvider m_Rng = new RNGCryptoServiceProvider();

        public string I { get; private set; }

        public BigInteger N { get; } = BigInteger.Parse("0894B645E89E1535BBDAD5B8B290650530801B18EBFBF5E8FAB3C82872A3E9BB7", NumberStyles.HexNumber); //"894B645E89E1535BBDAD5B8B290650530801B18EBFBF5E8FAB3C82872A3E9BB7".ToBigIntegerLittleEndian();
        // Generator
        public BigInteger g { get; } = 7;
        //
        public BigInteger k { get; } = 3;
        // Salt
        public BigInteger s { get; private set; }
        public BigInteger x { get; private set; }
        // Verifier
        public BigInteger v { get; private set; }
        public BigInteger b { get; private set; }
        public BigInteger B { get; private set; }

        public BigInteger A { get; set; }
        public BigInteger u { get { return H(A.ToProperByteArray(), B.ToProperByteArray()); } }
        public BigInteger S_s { get { return BigInteger.ModPow(A * BigInteger.ModPow(v, u, N), b, N); } }
        public BigInteger K_s { get { return Interleave(S_s); } } // SessionKey
        public BigInteger M_c { get; set; }
        public BigInteger M_s { get { return H(A.ToProperByteArray(), M_c.ToProperByteArray(), K_s.ToProperByteArray()); } } // Proof

        public BigInteger H(params byte[][] args)
        {
            return Sha1Hash(args.SelectMany(b => b).ToArray()).ToPositiveBigInteger();
        }

        public BigInteger GenerateClientProof(/*BigInteger K_c*/)
        {
            // M = H(H(N) xor H(g), H(I), s, A, B, K)
            var N_hash = Sha1Hash(N.ToProperByteArray());
            var g_hash = Sha1Hash(g.ToProperByteArray());
            var I_hash = Sha1Hash(Encoding.ASCII.GetBytes(I));

            // H(N) XOR H(g)
            for (int i = 0, j = N_hash.Length; i < j; i++)
                N_hash[i] ^= g_hash[i];

            return H(N_hash, I_hash, s.ToProperByteArray(), A.ToProperByteArray(), B.ToProperByteArray(), K_s.ToProperByteArray() /*, K_c.ToProperByteArray()*/);
        }

        // http://www.ietf.org/rfc/rfc2945.txt
        // Chapter 3.1
        public BigInteger Interleave(BigInteger K_s)
        {            
            var T = K_s.ToProperByteArray().SkipWhile(b => b == 0).ToArray(); // Remove all leading 0-bytes
            if ((T.Length & 0x1) == 0x1) T = T.Skip(1).ToArray(); // Needs to be an even length, skip 1 byte if not
            var G = Sha1Hash(Enumerable.Range(0, T.Length).Where(i => (i & 0x1) == 0x0).Select(i => T[i]).ToArray());
            var H = Sha1Hash(Enumerable.Range(0, T.Length).Where(i => (i & 0x1) == 0x1).Select(i => T[i]).ToArray());

            var result = new byte[40];
            for (int i = 0, r_c = 0; i < result.Length / 2; i++)
            {
                result[r_c++] = G[i];
                result[r_c++] = H[i];
            }

            return result.ToPositiveBigInteger();
        }

        private BigInteger GetRandomNumber(uint bytes)
        {
            var data = new byte[bytes];
            m_Rng.GetNonZeroBytes(data);
            return data.ToPositiveBigInteger();
        }

        public static byte[] Sha1Hash(byte[] bytes)
        {
            var sha1 = SHA1.Create();
            return sha1.ComputeHash(bytes);
        }

        public static void PrintBytes(byte[] bytes, string sep = "")
        {
            Console.WriteLine(string.Join(sep, bytes.Select(b => b.ToString("X2"))));
        }
    }

    internal static class SRPHelperExtensions
    {
        // ToByteArray appends a 0x00-byte to positive integers
        public static byte[] ToProperByteArray(this BigInteger b)
        {
            var bytes = b.ToByteArray();
            if (b.Sign == 1 && (bytes.Length > 1 && bytes[bytes.Length - 1] == 0))
                Array.Resize(ref bytes, bytes.Length - 1);
            return bytes;
        }

        // http://stackoverflow.com/a/5649264
        public static BigInteger ToPositiveBigInteger(this byte[] bytes)
        {
            return new BigInteger(bytes.Concat(new byte[] { 0 }).ToArray());
        }

        public static byte[] Pad(this byte[] bytes, int count)
        {
            Array.Resize(ref bytes, count);
            return bytes;
        }
    }
}
