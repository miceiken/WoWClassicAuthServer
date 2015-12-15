using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using WoWClassic.Common.DataStructure;
using WoWClassic.Common.Protocol;
using System.Linq;
using System;


namespace WoWClassic.Common.Network
{
    public interface IServer<T> where T : Connection
    {
        void Listen(IPEndPoint endPoint);
        List<T> Connections { get; }
    }
}
