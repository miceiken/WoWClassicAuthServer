using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoWClassic.Common.Constants
{
    public enum PacketDirection
    { // Value is header size
        ToClient = 4,
        FromClient = 6,
    }
}
