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
            s = GetRandomNumber(32) % N;
            //s = "BEA833881768877A7B8801DFE2C7DEEDDEA7860F57ABD04EFFCC672E67B462B9".ToBigIntegerLittleEndian();
            x = H(s.ToProperByteArray(), Encoding.ASCII.GetBytes(I), Encoding.ASCII.GetBytes(p));
            v = BigInteger.ModPow(g, x, N);
            //v = "2D536375F9E68F5049DCA0D2E9DCAE482B854F00B7A6689DEAEE33BC83320998".ToBigIntegerLittleEndian();

            b = GetRandomNumber(19) % N;
            //b = "1705509458079758617995494716578076552495725229055527278653649485839533168055".ToBigIntegerLittleEndian(NumberStyles.None);
            B = (k * v + BigInteger.ModPow(g, b, N)) % N;
        }

        private RNGCryptoServiceProvider m_Rng = new RNGCryptoServiceProvider();

        public BigInteger N { get; } = "894B645E89E1535BBDAD5B8B290650530801B18EBFBF5E8FAB3C82872A3E9BB7".ToBigIntegerLittleEndian();
        public BigInteger g { get; } = 7;
        public BigInteger k { get; } = 3;
        public BigInteger s { get; private set; }
        public BigInteger x { get; private set; }
        public BigInteger v { get; private set; }
        public BigInteger b { get; private set; }
        public BigInteger B { get; private set; }

        public BigInteger A { get; set; }
        public BigInteger u { get { return H(A.ToProperByteArray(), B.ToProperByteArray()); } }
        public BigInteger K_s { get { return Interleave(H(BigInteger.ModPow(A * BigInteger.ModPow(v, u, N), b, N).ToProperByteArray())); } } // SessionKey
        public BigInteger M_c { get; set; }
        public BigInteger M_s { get { return H(A.ToProperByteArray(), M_c.ToProperByteArray(), K_s.ToProperByteArray()); } } // Proof

        public BigInteger H(params byte[][] args)
        {
            return Sha1Hash(Encoding.ASCII.GetBytes(string.Join("", args.Select(bytes => bytes.Select(b => b.ToString("X2")))))).ToPositiveBigInteger();
        }

        // http://www.ietf.org/rfc/rfc2945.txt
        // Chapter 3.1
        public BigInteger Interleave(BigInteger K_s)
        {
            // Remove all leading 0-bytes
            var T = K_s.ToProperByteArray().SkipWhile(b => b == 0).ToArray();
            // Needs to be an even length, skip 1 byte if not
            if (T.Length % 2 == 1)
                T = T.Skip(1).ToArray();
            var E = new byte[T.Length / 2];
            var F = new byte[T.Length / 2];
            for (int i = 0, E_c = 0, F_c = 0; i < T.Length; i++)
            {
                if (i % 2 == 0)
                    E[E_c++] = T[i];
                else
                    F[F_c++] = T[i];
            }

            var G = Sha1Hash(E);
            var H = Sha1Hash(F);
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
            m_Rng.GetBytes(data);
            return data.ToPositiveBigInteger();
        }

        public static byte[] Sha1Hash(byte[] bytes)
        {
            var sha1 = SHA1.Create();
            return sha1.ComputeHash(bytes);
        }

        public static void PrintBytes(byte[] bytes)
        {
            Console.WriteLine(string.Join("", bytes.Select(b => b.ToString("X2"))));
        }
    }

    public static class BigIntegerExtensions
    {
        public static BigInteger ToBigIntegerLittleEndian(this string value, NumberStyles style = NumberStyles.HexNumber)
        {
            return BigInteger.Parse(value, style).ToByteArray().Reverse().ToArray().ToPositiveBigInteger();
        }

        // ToByteArray appends a 0x00-byte to positive integers
        public static byte[] ToProperByteArray(this BigInteger b)
        {
            var bytes = b.ToByteArray();
            if (b.Sign == -1 || (bytes.Length > 1 && bytes[bytes.Length - 1] == 0))
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
