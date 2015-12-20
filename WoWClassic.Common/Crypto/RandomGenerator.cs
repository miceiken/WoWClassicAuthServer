using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace WoWClassic.Common.Crypto
{
    public static class RandomGenerator
    {
        private static RNGCryptoServiceProvider s_RNGCrypto = new RNGCryptoServiceProvider();
        private static Random s_RNG = new Random();

        public static byte[] RandomBytes(int length)
        {
            var bytes = new byte[length];
            s_RNGCrypto.GetNonZeroBytes(bytes);
            return bytes;
        }

        public static int RandomInt()
        {
            return s_RNG.Next();
        }

    }
}
