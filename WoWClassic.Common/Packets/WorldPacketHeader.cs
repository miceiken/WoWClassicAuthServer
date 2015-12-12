using System;
using System.IO;
using WoWClassic.Common.Constants;
using WoWClassic.Common.Crypto;

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

        public WorldPacketHeader(BinaryReader br)
        { // We assume that it's already been decrypted
            Length = br.ReadUInt16();
            Opcode = (WorldOpcodes)br.ReadUInt32();
        }

        public ushort Length { get; private set; }
        public WorldOpcodes Opcode { get; private set; }

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
