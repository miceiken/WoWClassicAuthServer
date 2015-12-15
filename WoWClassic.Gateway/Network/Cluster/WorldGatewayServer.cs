using System;
using System.Collections.Generic;
using WoWClassic.Common.Network;

namespace WoWClassic.Gateway
{
    public class WorldGatewayServer : Server
    {
        public WorldGatewayServer()
        { }

        protected override void AcceptCallback(IAsyncResult ar)
        {
            Connections.Add(new WorldGatewayConnection(this, m_Listener.EndAccept(ar)));
            base.AcceptCallback(ar);
        }

        public Dictionary<ulong, GatewayConnection> GUIDClientMap { get; private set; } = new Dictionary<ulong, GatewayConnection>();
        public Dictionary<GatewayConnection, WorldGatewayConnection> ClientConnectionMap { get; private set; } = new Dictionary<GatewayConnection, WorldGatewayConnection>();
    }
}
