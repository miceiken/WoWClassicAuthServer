using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoWClassic.Common.Protocol
{
    public struct ServicePacketPair
    {
        public ServicePacketPair(byte service, byte packetId)
        {
            Service = service;
            PacketId = packetId;
        }

        public byte Service;
        public byte PacketId;
    }
}
