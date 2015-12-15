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
            using (var ms = new MemoryStream(data))
            using (var br = new BinaryReader(ms))
            {
                try
                {
                    var characterGUID = br.ReadUInt64();
                    WorldClient client;
                    if (!WorldManager.Instance.GUIDClientMap.TryGetValue(characterGUID, out client)) // Assume it's the first we see of this client
                        WorldManager.Instance.GUIDClientMap.Add(characterGUID, (client = new WorldClient(characterGUID)));

                    var header = new WorldPacketHeader(br);

                    Log.WriteLine(WorldLogTypes.Packets, $"<- {header.Opcode}({data.Length}):\n\t{string.Join(" ", data.Select(b => b.ToString("X2")))}");
                    if (!WorldHandler.PacketHandlers.ContainsKey(header.Opcode) || !WorldHandler.PacketHandlers[header.Opcode](client, br))
                        Log.WriteLine(WorldLogTypes.Packets, $"Failed to handle command {header.Opcode}");

                    return 8  + 2 + header.Length; // We read GUID, opcode +  header length
                }
                catch (EndOfStreamException) { return -1; }
            }
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
