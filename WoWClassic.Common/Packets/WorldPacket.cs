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
    public class WorldPacket
    {
        public ulong GUID;
        public WorldPacketHeader Header;
        public ArraySegment<byte> Payload;
        public int TotalLength = 0;

        public static IEnumerable<WorldPacket> FromBuffer(byte[] buffer, WorldPacketFlags flags = WorldPacketFlags.None, AuthCrypt crypt = null)
        {
            using (var ms = new MemoryStream(buffer))
            using (var br = new BinaryReader(ms))
            {
                while (ms.Position < buffer.Length)
                {
                    var start = ms.Position;
                    var pkt = new WorldPacket();
                    if (flags.HasFlag(WorldPacketFlags.GUIDPrefix))
                        pkt.GUID = br.ReadUInt64();

                    pkt.Header = new WorldPacketHeader();
                    if (flags.HasFlag(WorldPacketFlags.EncryptedHeader) && crypt != null)
                    {
                        // We do a null check, because the first received world packet is NOT encrypted
                        ms.Write(crypt.GetDecrypted(buffer, (int)ms.Position), (int)ms.Position, 6);
                        ms.Position -= 6;
                    }

                    if (flags.HasFlag(WorldPacketFlags.BigEndianLength)) // Client sends 6 byte header (4-byte opcode, 2-byte length)
                        pkt.Header.Length = BitConverter.ToUInt16(br.ReadBytes(2).Reverse().ToArray(), 0); // TODO: eew?
                    else
                        pkt.Header.Length = br.ReadUInt16();

                    // This means we have received a partial packet, can't pass that on
                    if ((ms.Position + pkt.Header.Length) > buffer.Length)
                        break;

                    if (flags.HasFlag(WorldPacketFlags.Outbound))
                        pkt.Header.Opcode = (WorldOpcodes)br.ReadUInt16();
                    else
                        pkt.Header.Opcode = (WorldOpcodes)br.ReadUInt32();

                    pkt.Payload = new ArraySegment<byte>(buffer, (int)ms.Position, pkt.Header.Length - (flags.HasFlag(WorldPacketFlags.Outbound) ? 2 : 4));
                    pkt.TotalLength = (int)(pkt.Header.Length - start);

                    yield return pkt;

                    ms.Seek(pkt.Header.Length, SeekOrigin.Current);
                }
            }
        }
    }

    [Flags]
    public enum WorldPacketFlags
    {
        // If header is encrypted, we need to decrypt it, and keep endian in mind
        EncryptedHeader = 1 << 0,
        // This means the header is in big endian, and we need to swap it
        BigEndianLength = 1 << 1,
        // Client expects 2-byte opcode, while it sends 4-byte
        Outbound = 1 << 2,
        // For cluster services we prepend packets with character guid
        GUIDPrefix = 1 << 3,

        None = 0
    }
}
