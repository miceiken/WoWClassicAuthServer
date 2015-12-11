using System.Collections.Generic;
using System.Linq;
using WoWClassic.Common;
using WoWClassic.Common.Constants;

namespace WoWClassic.World.WorldServer
{
    public class WorldConnection
    {
        public WorldConnection(WorldServer server)
        {
            m_Server = server;

            m_CommandHandlers = RegisterHandlers(this);

            OnAccept();
        }

        private readonly WorldServer m_Server;
        private readonly Dictionary<WorldOpcodes, CommandHandler> m_CommandHandlers;

        #region Packet Handler Reflection

        private static Dictionary<WorldOpcodes, CommandHandler> RegisterHandlers(object instance)
        {
            var ret = new Dictionary<WorldOpcodes, CommandHandler>();
            var type = instance.GetType();
            foreach (var method in type.GetMethods())
            {
                var attr = method.GetCustomAttributes(typeof(PacketHandlerAttribute), false).Cast<PacketHandlerAttribute>().FirstOrDefault();
                if (attr == null) continue;
                ret.Add((WorldOpcodes)attr.PacketId, (CommandHandler)method.CreateDelegate(typeof(CommandHandler), instance));
            }
            return ret;
        }

        #endregion

        #region Socket

        public void OnAccept()
        {
        }

        public void OnReceive()
        {
            //int bytesRead;
            //while ((bytesRead = m_Socket.Receive(m_RecvBuffer)) > 0)
            //{
            //    var buffer = new byte[bytesRead];
            //    Buffer.BlockCopy(m_RecvBuffer, 0, buffer, 0, bytesRead);

            //    using (var ms = new MemoryStream(buffer))
            //    using (var br = new BinaryReader(ms))
            //    {
            //        var header = new WorldPacketHeader(m_Crypt, br);

            //        Log.WriteLine(GatewayLogTypes.Packets, $"<- {header.Opcode}({buffer.Length}):\n\t{string.Join(" ", buffer.Select(b => b.ToString("X2")))}");
            //        if (!m_CommandHandlers.ContainsKey(header.Opcode) || !m_CommandHandlers[header.Opcode](br, header.Length - 6))
            //            Log.WriteLine(GatewayLogTypes.Packets, $"Failed to handle command {header.Opcode}");
            //    }
            //}
            //m_Server.Clients.Remove(this);
            //Console.WriteLine("Dropped connection from {0}", ((IPEndPoint)m_Socket.RemoteEndPoint).Address);
        }

        private void SendPacket(WorldOpcodes opcode, byte[] data)
        {
            //using (var ms = new MemoryStream())
            //using (var bw = new BinaryWriter(ms))
            //{
            //    bw.Write(((ushort)(data.Length + 2)).SwitchEndian());
            //    bw.Write((ushort)opcode);
            //    bw.Write(data);

            //    var packet = ms.ToArray();
            //    m_Crypt?.Encrypt(packet);
            //    Log.WriteLine(GatewayLogTypes.Packets, $"-> {opcode}({packet.Length}):\n\t{string.Join(" ", packet.Select(b => b.ToString("X2")))}");
            //    m_Socket.Send(packet);
            //}
        }

        #endregion
    }
}
