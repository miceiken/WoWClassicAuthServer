using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using WoWClassic.Common;
using WoWClassic.Common.Protocol;
using WoWClassic.Common.Packets;
using WoWClassic.Common.Constants;
using WoWClassic.Common.Log;
using WoWClassic.Common.Network;

namespace WoWClassic.World
{
    public class GatewayServerConnection : Connection
    {
        public GatewayServerConnection(Server server, Socket socket)
            : base(server, socket)
        { }

        protected override int ProcessInternal(byte[] data)
        {
            var packets = WorldPacket.FromBuffer(data, WorldPacketFlags.GUIDPrefix);
            var bytesRead = 0;
            foreach (var pkt in packets)
            {
                WorldClient client;
                if (!WorldManager.Instance.GUIDClientMap.TryGetValue(pkt.GUID, out client)) // Assume it's the first we see of this client
                    WorldManager.Instance.GUIDClientMap.Add(pkt.GUID, (client = new WorldClient(pkt.GUID)));

                using (var ms = new MemoryStream(pkt.Payload.ToArray()))
                using (var br = new BinaryReader(ms))
                {
                    Log.WriteLine(WorldLogTypes.Packets, $"<- {pkt.Header.Opcode}({pkt.Header.Length}):\n\t{string.Join(" ", pkt.Payload.Select(b => b.ToString("X2")))}");
                    if (!WorldHandler.PacketHandlers.ContainsKey(pkt.Header.Opcode) || !WorldHandler.PacketHandlers[pkt.Header.Opcode](client, br))
                        Log.WriteLine(WorldLogTypes.Packets, $"Failed to handle command {pkt.Header.Opcode}");
                }

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
