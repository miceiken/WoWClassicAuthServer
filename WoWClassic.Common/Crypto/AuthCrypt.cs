using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WoWClassic.Common.Crypto
{
    public class AuthCrypt
    {
        public AuthCrypt(byte[] sessionKey)
        {
            SessionKey = sessionKey;
        }

        private const int HEADER_RECEIVE_SIZE = 6;
        private const int HEADER_SEND_SIZE = 4;

        public byte[] SessionKey { get; private set; }

        private byte ei = 0, ej = 0;
        public void Encrypt(byte[] data, int offset = 0)
        {
            for (int i = offset; i < offset + HEADER_SEND_SIZE; i++)
            {
                ei = (byte)(ei % SessionKey.Length);
                var x = (byte)((data[i] ^ SessionKey[ei++]) + ej);
                data[i] = ej = x;
            }
        }

        private byte di = 0, dj = 0;
        public void Decrypt(byte[] data, int offset = 0)
        {
            for (int i = offset; i < offset + HEADER_RECEIVE_SIZE; i++)
            {
                di = (byte)(di % SessionKey.Length);
                var x = (byte)((data[i] - dj) ^ SessionKey[di++]);
                dj = data[i];
                data[i] = x;
            }
        }

        public byte[] GetDecrypted(byte[] data, int offset)
        {
            var decrypted = new byte[HEADER_RECEIVE_SIZE];
            for (var i = offset; i < offset + HEADER_RECEIVE_SIZE; i++)
            {
                di = (byte)(di % SessionKey.Length);
                var x = (byte)((data[i] - dj) ^ SessionKey[di++]);
                dj = data[i];
                decrypted[i] = x;
            }
            return decrypted;
        }
    }
}
