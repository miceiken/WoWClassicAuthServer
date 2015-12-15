using System;
using System.IO;
using System.Net.Sockets;
using WoWClassic.Common.Network;

namespace WoWClassic.Gateway
{
    public class WorldGatewayConnection : Connection
    {
        public WorldGatewayConnection(WorldGatewayServer server, Socket socket)
            : base(server, socket)
        { }

        protected override int ProcessInternal(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            using (var br = new BinaryReader(ms))
            {
                GatewayConnection client;
                if (!((WorldGatewayServer)m_Server).GUIDClientMap.TryGetValue(br.ReadUInt64(), out client))
                    throw new Exception("World server refers to unconnected character");

                var buffer = new byte[data.Length - 8];
                Buffer.BlockCopy(data, 7, buffer, 0, data.Length);
                client.SendPacket(buffer);

                return data.Length;
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
