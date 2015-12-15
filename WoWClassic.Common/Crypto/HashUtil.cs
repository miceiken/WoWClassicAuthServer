using System.Linq;
using System.Security.Cryptography;

namespace WoWClassic.Common.Crypto
{
    public static class HashUtil
    {
        public static byte[] ComputeHash(params byte[][] args)
        {
            using (var sha = SHA1.Create())
                return sha.ComputeHash(args.SelectMany(b => b).ToArray());
        }
    }
}
