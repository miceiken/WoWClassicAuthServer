using System;

namespace WoWClassicServer
{
    public static class Extensions
    {
        public static byte[] Pad(this byte[] bytes, int count)
        {
            Array.Resize(ref bytes, count);
            return bytes;
        }
    }
}
