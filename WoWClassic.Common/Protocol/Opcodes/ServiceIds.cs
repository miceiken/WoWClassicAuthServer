using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoWClassic.Common.Protocol
{
    public enum ServiceIds : byte
    {
        Gateway = 1,
        Login,
        World,
        Database,
    }
}
