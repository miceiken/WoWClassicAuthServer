using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using WoWClassicServer.AuthServer.Constants;

namespace WoWClassicServer.AuthServer
{
    public delegate bool CommandHandler(BinaryReader br);

    public class AuthConnection
    {
        public AuthConnection(Socket socket, AuthServer server)
        {
            m_Socket = socket;
            m_Server = server;

            m_CommandHandlers = new Dictionary<AuthCommand, CommandHandler>()
            {
                { AuthCommand.CMD_AUTH_LOGON_CHALLENGE, HandleLogonChallenge },
                { AuthCommand.CMD_AUTH_LOGON_PROOF, HandleLogonProof },
                { AuthCommand.CMD_AUTH_RECONNECT_CHALLENGE, HandleReconnectChallenge },
                { AuthCommand.CMD_AUTH_RECONNECT_PROOF, HandleReconnectProof },
                { AuthCommand.CMD_REALM_LIST, HandleRealmlist },
            };

            OnAccept();
        }

        private readonly Socket m_Socket;
        private readonly AuthServer m_Server;

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

                    Console.WriteLine("Received command: {0} - Length: {1}", Enum.GetName(typeof(AuthCommand), command), bytesRead);
                    if (m_CommandHandlers.ContainsKey(command))
                    {
                        if (!m_CommandHandlers[command](br))
                            Console.WriteLine("Failed to handle command {0}", command);
                    }
                    else
                        Console.WriteLine("Unknown packet. Length: {0}", bytesRead);
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

            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                bw.Write((byte)AuthCommand.CMD_AUTH_LOGON_CHALLENGE);
                bw.Write((byte)0x0);
                bw.Write((byte)AuthResult.WOW_SUCCESS); // TODO: Check for suspension/ipban/accountban

                // Insert voodoo shit with SRP6 here

                m_Socket.Send(ms.ToArray());
            }

            return true;
        }

        public bool HandleLogonProof(BinaryReader br)
        {
            return false;
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
