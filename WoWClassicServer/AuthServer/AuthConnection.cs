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
using System.Runtime.InteropServices;

namespace WoWClassicServer.AuthServer
{
    public delegate bool CommandHandler(BinaryReader br, int packetLength);

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

        private AuthLogonChallenge m_ALC;
        private AuthLogonProof m_ALP;
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
                        if (!m_CommandHandlers[command](br, bytesRead - 1))
                            Console.WriteLine("Failed to handle command {0}", command);
                    }
                    else
                        Console.WriteLine("Command({0}): (!) Unknown", bytesRead);
                }
            }
            m_Server.Clients.Remove(this);
            Console.WriteLine("Dropped connection from {0}", ((IPEndPoint)m_Socket.RemoteEndPoint).Address);
        }

        public bool WriteProof(BinaryWriter bw, ushort build)
        {
            switch (build)
            {
                case 5875:                                          // 1.12.1
                case 6005:                                          // 1.12.2
                case 6141:                                          // 1.12.3

                    bw.Write((byte)AuthCommand.AuthLogonProof);     // cmd
                    bw.Write((byte)0);                              // error
                    bw.Write(m_SRP.M_s.ToByteArray(), 0, 20);       // M2
                    bw.Write((uint)0x00);                           // unk1
                    bw.Write((uint)0x00);                           // unk2
                    bw.Write((ushort)0x00);                         // unk3

                    return true;
                case 8606:                                          // 2.4.3
                case 10505:                                         // 3.2.2a
                case 11159:                                         // 3.3.0a
                case 11403:                                         // 3.3.2
                case 11723:                                         // 3.3.3a
                case 12340:                                         // 3.3.5a
                default:
                    Console.WriteLine("Client has unsupported build ({0})", build);
                    return false;
            }
        }

        //public void LoadRealmlist() { }

        public bool HandleLogonChallenge(BinaryReader br, int packetLength)
        {
            // Sanity check
            if (AuthLogonChallenge.SizeConst > packetLength)
                return false;

            m_ALC = AuthLogonChallenge.Read(br);
            Console.WriteLine(m_ALC.ToString());

            // TODO: This is where we would get the password from database
            m_SRP = new SRP(m_ALC.I, string.Join("", SRP.Sha1Hash(Encoding.ASCII.GetBytes(m_ALC.I + ":" + m_ALC.I)).Select(b => b.ToString("X2"))));

            Console.WriteLine("I={0}", m_ALC.I);
            Console.WriteLine("p={0}", string.Join("", SRP.Sha1Hash(Encoding.ASCII.GetBytes(m_ALC.I + ":" + m_ALC.I)).Select(b => b.ToString("X2"))));
            Console.Write("v=");
            SRP.PrintBytes(m_SRP.v.ToProperByteArray());

            var B = m_SRP.B.ToProperByteArray().Pad(32);
            Console.Write("B=");
            SRP.PrintBytes(B);

            var N = m_SRP.N.ToProperByteArray().Pad(32);
            Console.Write("N=");
            SRP.PrintBytes(N);

            var s = m_SRP.s.ToProperByteArray().Pad(32);            
            Console.Write("s=");
            SRP.PrintBytes(s);

            Console.Write("g=");
            SRP.PrintBytes(m_SRP.g.ToByteArray());

            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                bw.Write((byte)AuthCommand.AuthLogonChallenge);
                bw.Write((byte)0x0);
                bw.Write((byte)AuthResult.WOW_SUCCESS); // TODO: Check for suspension/ipban/accountban
                bw.Write(B);
                bw.Write((byte)1);
                bw.Write(m_SRP.g.ToByteArray());
                bw.Write((byte)32);
                bw.Write(N);
                bw.Write(s);
                bw.Write(new byte[16]);
                bw.Write((byte)0); // security flags

                Thread.Sleep(100);

                m_Socket.Send(ms.ToArray());
            }

            return true;
        }

        public bool HandleLogonProof(BinaryReader br, int packetLength)
        {
            // Sanity check
            if (AuthLogonProof.SizeConst > packetLength)
                return false;

            m_ALP = AuthLogonProof.Read(br);
            Console.WriteLine(m_ALP.ToString());

            m_SRP.A = m_ALP.A.ToPositiveBigInteger();
            m_SRP.M_c = m_ALP.M1.ToPositiveBigInteger();

            Console.Write("A=");
            SRP.PrintBytes(m_SRP.A.ToProperByteArray());

            Console.Write("M_c=");
            SRP.PrintBytes(m_SRP.M_c.ToProperByteArray());

            Console.Write("K_s=");
            SRP.PrintBytes(m_SRP.K_s.ToProperByteArray());

            Console.Write("M_s=");
            SRP.PrintBytes(m_SRP.M_s.ToProperByteArray());

            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                if (!WriteProof(bw, m_ALC.Build))
                    return false;

                Thread.Sleep(100);

                m_Socket.Send(ms.ToArray());
            }

            return true;
        }

        public bool HandleReconnectChallenge(BinaryReader br, int packetLength)
        {
            return false;
        }

        public bool HandleReconnectProof(BinaryReader br, int packetLength)
        {
            return false;
        }

        public bool HandleRealmlist(BinaryReader br, int packetLength)
        {
            return false;
        }
    }
}
