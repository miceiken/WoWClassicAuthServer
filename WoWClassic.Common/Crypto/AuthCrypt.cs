using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public void Encrypt(byte[] data)
        {
            for (int i = 0; i < HEADER_SEND_SIZE; i++)
            {
                ei = (byte)(ei % SessionKey.Length);
                var x = (byte)((data[i] ^ SessionKey[ei++]) + ej);
                data[i] = ej = x;
            }
        }

        private byte di = 0, dj = 0;
        public void Decrypt(byte[] data)
        {
            for (int i = 0; i < HEADER_RECEIVE_SIZE; i++)
            {
                di = (byte)(di % SessionKey.Length);
                var x = (byte)((data[i] - dj) ^ SessionKey[di++]);
                dj = data[i];
                data[i] = x;
            }
        }
    }
}
