using System;
using System.IO;
using System.Net.Sockets;

namespace WoWClassic.Common.Network
{
    public class Connection : IConnection
    {
        public Connection(Server server, Socket socket)
        {
            m_Server = server;
            m_Socket = socket;

            var state = new StateObject { Socket = m_Socket };
            m_Socket.BeginReceive(state.Buffer, 0, StateObject.BufferSize, SocketFlags.None, ReceiveCallback, state);
        }

        protected readonly Server m_Server;
        protected readonly Socket m_Socket;

        private void ReceiveCallback(IAsyncResult ar)
        {
            var state = (StateObject)ar.AsyncState;

            int length;
            if ((length = state.Socket.EndReceive(ar)) == 0)
            { // Connection closed
                m_Server?.Connections.Remove(this);
                return;
            }

            Process(state.Buffer, length);
            m_Socket.BeginReceive(state.Buffer, 0, StateObject.BufferSize, SocketFlags.None, ReceiveCallback, state);
        }

        public void Process(byte[] data, int length)
        {
            var buffer = new byte[length];
            Buffer.BlockCopy(data, 0, buffer, 0, length);

            int handled = 0, next;
            while (handled < length)
            {
                if ((next = ProcessInternal(data)) == -1) break;
                handled += next;
            }
        }

        // Returns true if it managed to read full packet, false if not
        protected virtual int ProcessInternal(byte[] data) { return 0; }

        public virtual void Send(byte[] data)
        {
            m_Socket.BeginSend(data, 0, data.Length, SocketFlags.None, SendCallback, m_Socket);
        }

        private void SendCallback(IAsyncResult ar)
        {
            var handler = (Socket)ar.AsyncState;
            var sent = handler.EndSend(ar);
        }

        public class StateObject
        {
            public Socket Socket = null;
            public const int BufferSize = 1024;
            public byte[] Buffer = new byte[BufferSize];
        }
    }
}
