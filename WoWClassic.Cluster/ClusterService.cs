using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using WoWClassic.Common;
using WoWClassic.Common.Protocol;

namespace WoWClassic.Cluster
{
    public abstract class ClusterService<T> where T : struct
    {
        public ClusterService(ServiceIds serviceId)
        {
            m_ServiceId = serviceId;

            m_CommandHandlers = RegisterHandlers(this);
        }

        #region Packet Handler Reflection

        private static Dictionary<Type, ServiceIds> s_PacketServiceIds = new Dictionary<Type, ServiceIds>()
        {
            [typeof(GatewayServicePacketIds)] = ServiceIds.Gateway,
            [typeof(LoginServicePacketIds)] = ServiceIds.Login,
            [typeof(WorldServicePacketIds)] = ServiceIds.World,
            [typeof(DatabaseServicePacketIds)] = ServiceIds.Database,
        };

        private static Dictionary<ServiceIds, Type> s_ServicePacketIds = new Dictionary<ServiceIds, Type>()
        {
            [ServiceIds.Gateway] = typeof(GatewayServicePacketIds),
            [ServiceIds.Login] = typeof(LoginServicePacketIds),
            [ServiceIds.World] = typeof(WorldServicePacketIds),
            [ServiceIds.Database] = typeof(DatabaseServicePacketIds),
        };

        private static Dictionary<ServicePacketPair, CommandHandler> RegisterHandlers(object instance)
        {
            var ret = new Dictionary<ServicePacketPair, CommandHandler>();
            var type = instance.GetType();
            foreach (var method in type.GetMethods())
            {
                var attr = method.GetCustomAttributes(typeof(PacketHandlerAttribute), true).Cast<PacketHandlerAttribute>().FirstOrDefault();
                if (attr == null) continue;
                ret.Add(new ServicePacketPair((byte)s_PacketServiceIds[attr.PacketType], (byte)attr.PacketId), (CommandHandler)method.CreateDelegate(typeof(CommandHandler), instance));
            }
            return ret;
        }

        #endregion

        private const int CLUSTER_PORT = 2245;

        private static IPAddress s_MulticastAddress = IPAddress.Parse("239.255.0.1");
        private static IPEndPoint s_Local = new IPEndPoint(IPAddress.Any, CLUSTER_PORT);
        private static IPEndPoint s_Remote = new IPEndPoint(s_MulticastAddress, CLUSTER_PORT);


        private UdpClient m_Client;
        private Thread m_Listener;

        private byte[] m_RecvBuffer = new byte[1024];

        private ServiceIds m_ServiceId;
        private Dictionary<ServicePacketPair, CommandHandler> m_CommandHandlers;


        public void Participate()
        {
            m_Client = new UdpClient(AddressFamily.InterNetwork) { ExclusiveAddressUse = false, MulticastLoopback = true };
            m_Client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            m_Client.Client.Bind(s_Local);
            m_Client.JoinMulticastGroup(s_MulticastAddress);

            m_Listener = new Thread(OnReceive);
            m_Listener.Start();
        }

        public void Announce(T packetId, byte[] data)
        {
            Announce((byte)(object)packetId, data);
        }

        public virtual void Announce(byte packetId, byte[] data)
        {
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                bw.Write((byte)m_ServiceId);
                bw.Write(packetId);
                bw.Write(data);

                var packet = ms.ToArray();
                using (var sendClient = new UdpClient(AddressFamily.InterNetwork) { ExclusiveAddressUse = false, MulticastLoopback = true })
                {
                    sendClient.JoinMulticastGroup(s_MulticastAddress);
                    sendClient.Send(packet, packet.Length, s_Remote);
                }
            }
        }

        private void OnReceive()
        {
            Console.WriteLine("CLUSTER: Listening on UDP Multicast");
            byte[] buffer;
            while ((buffer = m_Client.Receive(ref s_Local)) != null)
            {
                using (var ms = new MemoryStream(buffer))
                using (var br = new BinaryReader(ms))
                {
                    var command = new ServicePacketPair(br.ReadByte(), br.ReadByte());

                    if (m_CommandHandlers.ContainsKey(command))
                    {
                        Console.WriteLine("Command({0}): {1}:{2}", buffer.Length, Enum.GetName(typeof(ServiceIds), command.Service), Enum.GetName(s_ServicePacketIds[(ServiceIds)command.Service], command.PacketId));
                        if (!m_CommandHandlers[command](br, buffer.Length - 2))
                            Console.WriteLine("Failed to handle command {0}", command);
                    }
                    else
                        Console.WriteLine("Command({0}): {1}:{2} (!) No handler", buffer.Length, Enum.GetName(typeof(ServiceIds), command.Service), Enum.GetName(s_ServicePacketIds[(ServiceIds)command.Service], command.PacketId));
                }
            }
        }
    }
}
