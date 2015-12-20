using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using WoWClassic.Common.Network;
using WoWClassic.Common.Packets;
using WoWClassic.Common.Constants;
using WoWClassic.Common.Log;

namespace WoWClassic.Gateway
{
    public class WorldGatewayConnection : Connection
    {
        public WorldGatewayConnection(WorldGatewayServer server, Socket socket)
            : base(server, socket)
        { }

        protected override int ProcessInternal(byte[] data)
        {
            var packets = WorldPacket.FromBuffer(data, flags: WorldPacketFlags.GUIDPrefix | WorldPacketFlags.Outbound);
            var bytesRead = 0;
            foreach (var pkt in packets)
            {
                GatewayConnection client;
                if (!((WorldGatewayServer)m_Server).GUIDClientMap.TryGetValue(pkt.GUID, out client))
                    throw new Exception("World server refers to unconnected character");

                // We need to reconstruct the entire packet and not just the payload, in other words, include the 4-byte (2 length, 2 opcode) header
                var packet = new ArraySegment<byte>(pkt.Payload.Array, pkt.Payload.Offset - 4, pkt.Payload.Count + 4);

                Log.WriteLine(GatewayLogTypes.Packets, $"Forwarding {pkt.Header.Opcode} to client");
                client.SendPacket(packet.ToArray());

                bytesRead += pkt.TotalLength;
            }

            return bytesRead;
        }

        public void SendPacket(ulong guid, byte[] data)
        {
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                bw.Write(guid);
                bw.Write(data);
                Send(ms.ToArray());
            }
        }
    }
}
