using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using WoWClassic.Common.Crypto;
using WoWClassic.Common.Constants;

namespace WoWClassic.Common.Packets
{
    public class WorldPacketHeader
    {
        public WorldPacketHeader(AuthCrypt crypt, BinaryReader br)
        {
            var header = br.ReadBytes(6);
            crypt?.Decrypt(header);
            Length = BitConverter.ToUInt16(new byte[] { header[1], header[0] }, 0);
            Opcode = (WorldOpcodes)BitConverter.ToUInt32(new byte[] { header[2], header[3], header[4], header[5] }, 0);
        }

        public ushort Length { get; private set; }
        public WorldOpcodes Opcode { get; private set; }
    }
}
