using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoWClassic.Common.Constants
{
    public enum HighGUIDType : uint
    {
        Item = 0x4000,
        Container = 0x4000,
        Player = 0x0000,
        GameObject = 0xF110,
        Transport = 0xF120,
        Unit = 0xF130,
        Pet = 0xF140,
        DynamicObject = 0xF100,
        Corpse = 0xF101,
        MapObjectTransport = 0x1FC0,
    }
}
