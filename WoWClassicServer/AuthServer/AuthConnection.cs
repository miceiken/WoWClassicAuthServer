using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Linq;
using WoWClassicServer.AuthServer.Constants;
using WoWClassicServer.Crypto;
using System.Text;
using System.Numerics;

namespace WoWClassicServer.AuthServer
{
    public delegate bool CommandHandler(BinaryReader br);

    public class AuthConnection
    {
        public AuthConnection(Socket socket, AuthServer server)
        {
            m_Socket = socket;
            m_Server = server;

            // TODO: make these static? less overhead
            m_CommandHandlers = new Dictionary<AuthCommand, CommandHandler>()
            {
                { AuthCommand.AuthLogonChallenge, HandleLogonChallenge },
                { AuthCommand.AuthLogonProof, HandleLogonProof },
                { AuthCommand.AuthReconnectChallenge, HandleReconnectChallenge },
                { AuthCommand.AuthReconnectProof, HandleReconnectProof },
                { AuthCommand.RealmList, HandleRealmlist },
            };

            OnAccept();
        }

        private readonly Socket m_Socket;
        private readonly AuthServer m_Server;
        private SRP m_SRP;

        private readonly Dictionary<AuthCommand, CommandHandler> m_CommandHandlers;

        private Thread m_ThreadReceive;
        private byte[] m_RecvBuffer = new byte[1024];

        public void OnAccept()
        {
            Console.WriteLine("Accepting connection from {0}", ((IPEndPoint)m_Socket.RemoteEndPoint).Address);

            m_ThreadReceive = new Thread(OnReceive);
            m_ThreadReceive.Start();
        }

        public void OnReceive()
        {
            int bytesRead;
            while ((bytesRead = m_Socket.Receive(m_RecvBuffer, m_RecvBuffer.Length, SocketFlags.None)) > 0)
            {
                var buffer = new byte[bytesRead];
                Buffer.BlockCopy(m_RecvBuffer, 0, buffer, 0, bytesRead);

                using (var ms = new MemoryStream(buffer))
                using (var br = new BinaryReader(ms))
                {
                    var command = (AuthCommand)br.ReadByte();

                    Console.WriteLine("Command({1}): {0}", Enum.GetName(typeof(AuthCommand), command), bytesRead);
                    if (m_CommandHandlers.ContainsKey(command))
                    {
                        if (!m_CommandHandlers[command](br))
                            Console.WriteLine("Failed to handle command {0}", command);
                    }
                    else
                        Console.WriteLine("Command({0}): (!) Unknown", bytesRead);
                }
            }
            m_Server.Clients.Remove(this);
            Console.WriteLine("Dropped connection from {0}", ((IPEndPoint)m_Socket.RemoteEndPoint).Address);
        }

        //public void SendProof() { }
        //public void LoadRealmlist() { }

        public bool HandleLogonChallenge(BinaryReader br)
        {
            var alc = AuthLogonChallenge.Read(br);
            Console.WriteLine(alc.ToString());

            m_SRP = new SRP(alc.I, string.Join("", SRP.Sha1Hash(alc.I + ":" + alc.I).Select(b => b.ToString("X2"))));
            var B = m_SRP.B.ToByteArray();
            Array.Resize(ref B, 32);

            var N = m_SRP.N.ToByteArray();
            Array.Resize(ref N, 32);

            var s = m_SRP.s.ToByteArray();
            Array.Resize(ref s, 32);

            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                bw.Write((byte)AuthCommand.AuthLogonChallenge);
                bw.Write((byte)0x0);
                bw.Write((byte)AuthResult.WOW_SUCCESS); // TODO: Check for suspension/ipban/accountban
                bw.Write(B);
                bw.Write((byte)1);
                bw.Write(m_SRP.g.ToByteArray(), 0, 1);
                bw.Write((byte)32);
                bw.Write(N);
                bw.Write(s);
                bw.Write(new byte[16]);
                bw.Write((byte)0); // security flags

                m_Socket.Send(ms.ToArray());
            }

            return true;
        }

        public bool HandleLogonProof(BinaryReader br)
        {
            var alp = AuthLogonProof.Read(br);
            Console.WriteLine(alp.ToString());

            // https://github.com/cmangos/mangos-classic/blob/master/src/realmd/AuthSocket.cpp#L603
            m_SRP.A = new BigInteger(alp.A);
            m_SRP.M_c = new BigInteger(alp.M1);

            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                m_Socket.Send(ms.ToArray());
            }

            return true;
        }

        public bool HandleReconnectChallenge(BinaryReader br)
        {
            return false;
        }

        public bool HandleReconnectProof(BinaryReader br)
        {
            return false;
        }

        public bool HandleRealmlist(BinaryReader br)
        {
            return false;
        }
    }
}
