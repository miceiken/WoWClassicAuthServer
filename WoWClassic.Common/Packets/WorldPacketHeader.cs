using System;
using System.IO;
using WoWClassic.Common.Constants;
using WoWClassic.Common.Crypto;

namespace WoWClassic.Common.Packets
{
    public class WorldPacketHeader
    {
        public ushort Length;
        public WorldOpcodes Opcode;

        public byte[] GetDecrypted()
        {
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                bw.Write(Length);
                bw.Write((uint)Opcode);
                return ms.ToArray();
            }
        }
    }
}
