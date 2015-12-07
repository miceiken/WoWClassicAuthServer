using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoWClassic.Common.Crypto
{
    public class ARC4
    {
        public ARC4(byte[] key)
        {
            int i, j = 0;
            byte tmp;
            for (i = 0; i < 256; i++)
                m_State[i] = (byte)i;
            for (i = 0; i < 256; i++)
            {
                j = (j + m_State[i] + key[i % key.Length]) % 256;
                tmp = m_State[j];
                m_State[j] = m_State[i];
                m_State[i] = tmp;
            }
        }

        private byte[] m_State = new byte[256];
    }
}
