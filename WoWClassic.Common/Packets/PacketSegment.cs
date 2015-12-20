using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoWClassic.Common.Packets
{
    public class PacketSegment
    {
        public bool Decrypted;
        public ArraySegment<byte> Header;
        public ArraySegment<byte> Payload;
        public WorldPacketFlags Flags;
    }
}
