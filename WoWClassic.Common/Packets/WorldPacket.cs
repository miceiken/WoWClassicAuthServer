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
            using (var bw = new BinaryWriter(ms))
            {
                while (ms.Position < buffer.Length)
                {
                    var pkt = new WorldPacket();
                    if (flags.HasFlag(WorldPacketFlags.GUIDPrefix))
                    {
                        pkt.GUID = br.ReadUInt64();
                        pkt.TotalLength += 8;
                    }

                    pkt.Header = new WorldPacketHeader();

                    var headerSize = flags.HasFlag(WorldPacketFlags.Outbound) ? 4 : 6;
                    var headerBytes = br.ReadBytes(headerSize);
                    pkt.TotalLength += headerSize;

                    if (flags.HasFlag(WorldPacketFlags.EncryptedHeader) && crypt != null)
                    {
                        // We do a null check, because the first received world packet is NOT encrypted
                        //Console.WriteLine($"Encrypted: {string.Join(" ", headerBytes.Select(b => b.ToString("X2")))}");
                        crypt.Decrypt(headerBytes);
                        bw.Seek(-6, SeekOrigin.Current);
                        bw.Write(headerBytes);
                        //Console.WriteLine($"Decrypted: {string.Join(" ", headerBytes.Select(b => b.ToString("X2")))}");
                    }

                    pkt.Header.Length = BitConverter.ToUInt16(headerBytes, 0);
                    if (flags.HasFlag(WorldPacketFlags.BigEndianLength)) // Client sends 6 byte header (4-byte opcode, 2-byte length)
                        pkt.Header.Length = pkt.Header.Length.SwitchEndian(); // TODO: eew?

                    //Console.WriteLine($"Length: {pkt.Header.Length}");


                    // This means we have received a partial packet, can't pass that on
                    if (((ms.Position - headerSize + 2) + pkt.Header.Length) > buffer.Length)
                        break;

                    pkt.Header.Opcode = flags.HasFlag(WorldPacketFlags.Outbound) ?
                        (WorldOpcodes)BitConverter.ToUInt16(headerBytes, 2) : (WorldOpcodes)BitConverter.ToUInt32(headerBytes, 2);
                    //Console.WriteLine($"Opcode: {pkt.Header.Opcode}");

                    var payloadLength = pkt.Header.Length - (flags.HasFlag(WorldPacketFlags.Outbound) ? 2 : 4);
                    pkt.TotalLength += payloadLength;
                    pkt.Payload = new ArraySegment<byte>(buffer, (int)ms.Position, payloadLength);

                    yield return pkt;

                    ms.Seek(payloadLength, SeekOrigin.Current);
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
