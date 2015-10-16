using System;
using System.Linq;
using System.Numerics;

namespace WoWClassicAuthServer
{
    public static class Extensions
    {
        public static byte[] Pad(this byte[] bytes, int count)
        {
            Array.Resize(ref bytes, count);
            return bytes;
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
    }
}
