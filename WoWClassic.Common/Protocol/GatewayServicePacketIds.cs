using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoWClassic.Common.Protocol
{
    public enum GatewayServicePacketIds : byte
    {
        Announce = 1,

        UpdateRealm,
    }
}
