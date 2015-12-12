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

namespace WoWClassic.World
{
    public class GatewayServerConnection
    {
        public GatewayServerConnection(Socket socket)
        {
            m_Socket = socket;

            OnAccept();
        }

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
            int bytesRead;
            try
            {
                while ((bytesRead = m_Socket.Receive(m_RecvBuffer)) > 0)
                {
                    var buffer = new byte[bytesRead];
                    Buffer.BlockCopy(m_RecvBuffer, 0, buffer, 0, bytesRead);

                    using (var ms = new MemoryStream(buffer))
                    using (var br = new BinaryReader(ms))
                    {
                        var characterGUID = br.ReadUInt64();
                        Client client;
                        if (!WorldManager.Instance.GUIDClientMap.TryGetValue(characterGUID, out client)) // Assume it's the first we see of this client
                            WorldManager.Instance.GUIDClientMap.Add(characterGUID, (client = new Client(characterGUID)));

                        var header = new WorldPacketHeader(br);

                        Log.WriteLine(WorldLogTypes.Packets, $"<- {header.Opcode}({buffer.Length}):\n\t{string.Join(" ", buffer.Select(b => b.ToString("X2")))}");
                        if (!WorldHandler.PacketHandlers.ContainsKey(header.Opcode) || !WorldHandler.PacketHandlers[header.Opcode](client, br))
                            Log.WriteLine(WorldLogTypes.Packets, $"Failed to handle command {header.Opcode}");
                    }
                }
            }
            catch (SocketException e)
            {

            }
            finally
            {
                // TODO: what about the connection?
            }
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
