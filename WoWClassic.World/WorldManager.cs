using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWClassic.Common;
using WoWClassic.Cluster;
using WoWClassic.Common.Protocol;
using WoWClassic.Common.Packets;
using WoWClassic.Common.Log;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace WoWClassic.World
{
    public class WorldManager : Singleton<WorldManager>
    {
        public void Initialize()
        {
            Service = new WorldService();
            Service.RealmStatusChanged += OnRealmStatusChanged;
            Service.Participate();
        }

        public Dictionary<ulong, WorldClient> GUIDClientMap { get; private set; } = new Dictionary<ulong, WorldClient>();

        public WorldService Service { get; private set; }
        public GatewayServerConnection Gateway { get; private set; }

        public void OnRealmStatusChanged(object sender, EventArgs e)
        {
            Log.WriteLine(WorldLogTypes.General, $"Realm status changed for #{Service.RealmState.ID} '{Service.RealmState.Realm.Name}' -- Status: {Service.RealmState.Status}");
            if (Service.RealmState.Status == RealmStatus.Online)
            {
                if (Gateway != null)
                    return;
                var gatewayAddress = IPAddress.Parse(Service.RealmState.Realm.Address.Split(':')[0]);
                Log.WriteLine(WorldLogTypes.General, $"Connecting to gateway server at {gatewayAddress}:{Service.RealmState.GatewayPort}");

                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(new IPEndPoint(gatewayAddress, Service.RealmState.GatewayPort));
                Gateway = new GatewayServerConnection(null, socket);
            }
            // TODO: What to do when server goes offline?
        }
    }
}
