using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace WoWClassic.Common.Network
{
    public abstract class Server
    {
        public Server()
        {
            m_Listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        protected Socket m_Listener;
        private Thread m_AcceptThread;

        public List<Connection> Connections { get; private set; } = new List<Connection>();

        public virtual void Listen(IPEndPoint endPoint)
        {
            m_Listener.Bind(endPoint);
            m_Listener.Listen(2048);

            m_AcceptThread = new Thread(AcceptLoop);
            m_AcceptThread.Start();
        }

        private void AcceptLoop()
        {
           m_Listener.BeginAccept(AcceptCallback, null);
        }

        protected virtual void AcceptCallback(IAsyncResult ar)
        {
            m_Listener.BeginAccept(AcceptCallback, null);
        }
    }
}
