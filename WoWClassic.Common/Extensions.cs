using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.IO;

namespace WoWClassic.Common
{
    public static class Extensions
    {
        public static string ReadCString(this BinaryReader br)
        {
            var chars = new List<char>();
            char c;
            while ((c = br.ReadChar()) != '\0') chars.Add(c);
            return new string(chars.ToArray());
        }

        public static byte[] Pad(this byte[] bytes, int count)
        {
            Array.Resize(ref bytes, count);
            return bytes;
        }
    }

    public static class SRPHelperExtensions
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
