using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

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

        public static string UppercaseFirst(this string s)
        {
            return char.ToUpper(s[0]) + s.Substring(1).ToLower();
        }

        public static ulong ReadPacketGUID(this BinaryReader br)
        {
            var mask = br.ReadByte();
            if (mask == 0) return 0;
            var res = 0ul;
            for (var i = 0; i < 8; i++)
                if ((mask & (1 << i)) > 0)
                    res += (ulong)(br.ReadByte() << (i * 8));
            return res;
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

        public static ushort SwitchEndian(this ushort old)
        {
            var tmp = BitConverter.GetBytes(old);
            Array.Reverse(tmp);
            return BitConverter.ToUInt16(tmp, 0);
        }
    }
}
