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

namespace WoWClassic.Gateway
{
    public class WorldGatewayConnection
    {
        public WorldGatewayConnection(Socket socket, WorldGatewayServer server)
        {
            m_Server = server;
            m_Socket = socket;

            OnAccept();
        }

        private readonly WorldGatewayServer m_Server;
        private readonly Socket m_Socket;

        private Thread m_ThreadReceive;
        private byte[] m_RecvBuffer = new byte[1024];

        #region Socket

        public void OnAccept()
        {
            Console.WriteLine("Accepting connection from {0}", ((IPEndPoint)m_Socket.RemoteEndPoint).Address);

            m_ThreadReceive = new Thread(OnReceive);
            m_ThreadReceive.Start();
        }

        public void OnReceive()
        {
            int length;
            while ((length = m_Socket.Receive(m_RecvBuffer)) > 0)
            {
                var buffer = new byte[length];
                Buffer.BlockCopy(m_RecvBuffer, 0, buffer, 0, length);

                using (var ms = new MemoryStream(buffer))
                using (var br = new BinaryReader(ms))
                {
                    GatewayConnection client;
                    if (!m_Server.GUIDClientMap.TryGetValue(br.ReadUInt64(), out client))
                        //throw new Exception("World server refers to unconnected character");
                        continue;

                    buffer = new byte[length - 8];
                    Buffer.BlockCopy(m_RecvBuffer, 7, buffer, 0, length);
                    client.SendPacket(buffer);
                }
            }
            // TODO: reassign clients
            m_Server.Connections.Remove(this);
        }

        public void SendPacket(byte[] data)
        {
            m_Socket.Send(data);
        }

        public void SendPacket(ulong guid, byte[] data)
        {
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                bw.Write(guid);
                bw.Write(data);
                SendPacket(ms.ToArray());
            }
        }

        #endregion
    }
}
