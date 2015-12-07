using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using WoWClassic.Common;
using WoWClassic.Common.Constants;
using WoWClassic.Common.Crypto;
using WoWClassic.Common.Protocol;
using WoWClassic.Common.Packets;
using System.Security.Cryptography;
using WoWClassic.Cluster;

namespace WoWClassic.Gateway
{
    public class GatewayConnection
    {
        public GatewayConnection(Socket socket, GatewayServer server)
        {
            m_Socket = socket;
            m_Server = server;

            m_CommandHandlers = RegisterHandlers(this);

            OnAccept();
        }

        private static Random s_Rnd = new Random();

        private readonly Socket m_Socket;
        private readonly GatewayServer m_Server;

        private Thread m_ThreadReceive;
        private byte[] m_RecvBuffer = new byte[1024];

        private int m_Seed = s_Rnd.Next();
        private AuthCrypt m_Crypt;

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

        public void OnAccept()
        {
            Console.WriteLine("Accepting connection from {0}", ((IPEndPoint)m_Socket.RemoteEndPoint).Address);

            m_ThreadReceive = new Thread(OnReceive);
            m_ThreadReceive.Start();
        }

        public void OnReceive()
        {
            HandleAcceptedConnection();

            int bytesRead;
            while ((bytesRead = m_Socket.Receive(m_RecvBuffer, m_RecvBuffer.Length, SocketFlags.None)) > 0)
            {
                var buffer = new byte[bytesRead];
                Buffer.BlockCopy(m_RecvBuffer, 0, buffer, 0, bytesRead);

                using (var ms = new MemoryStream(buffer))
                using (var br = new BinaryReader(ms))
                {
                    var header = new WorldPacketHeader(m_Crypt, br);
                    Console.WriteLine($"<- {header.Opcode}({header.Length}|{buffer.Length})");
                    if (m_CommandHandlers.ContainsKey(header.Opcode))
                    {
                        if (!m_CommandHandlers[header.Opcode](br, header.Length - 6))
                            Console.WriteLine($"Failed to handle command {header.Opcode}");
                    }
                }
            }
            m_Server.Clients.Remove(this);
            Console.WriteLine("Dropped connection from {0}", ((IPEndPoint)m_Socket.RemoteEndPoint).Address);
        }

        public void HandleAcceptedConnection()
        {
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                bw.Write(m_Seed);
                SendPacket(WorldOpcodes.SMSG_AUTH_CHALLENGE, ms.ToArray());
            }
        }

        private void SendPacket(WorldOpcodes opcode, byte[] data)
        {
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                bw.Write(((ushort)(data.Length + 2)).SwitchEndian());
                bw.Write((ushort)opcode);
                bw.Write(data);

                var packet = ms.ToArray();
                m_Crypt?.Encrypt(packet);
                Console.WriteLine($"-> {opcode}({data.Length}|{packet.Length})");
                m_Socket.Send(packet);
            }
        }

        [PacketHandler(WorldOpcodes.CMSG_AUTH_SESSION)]
        public bool HandleAuthSession(BinaryReader br, int bytesRead)
        {
            var build = br.ReadUInt32();
            br.ReadUInt32();
            var account = br.ReadCString();
            var clientSeed = br.ReadUInt32();
            var clientDigest = br.ReadBytes(20);

            m_Crypt = new AuthCrypt(LoginService.GetSessionKey(account));

            var serverDigest = ComputeHash(Encoding.ASCII.GetBytes(account),
                new byte[] { 0, 0, 0, 0 },
                BitConverter.GetBytes(clientSeed),
                BitConverter.GetBytes(m_Seed),
                m_Crypt.SessionKey);
            if (!serverDigest.SequenceEqual(clientDigest))
                return false;

            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                bw.Write((byte)ResponseCodes.AUTH_OK);
                bw.Write((uint)0); // BillingTimeRemaining
                bw.Write((byte)0); // BillingPlanFlags
                bw.Write((uint)0); // BillingTimeRested

                SendPacket(WorldOpcodes.SMSG_AUTH_RESPONSE, ms.ToArray());
            }

            return true;
        }

        private static byte[] ComputeHash(params byte[][] args)
        {
            using (var sha = SHA1.Create())
                return sha.ComputeHash(args.SelectMany(b => b).ToArray());
        }
    }
}