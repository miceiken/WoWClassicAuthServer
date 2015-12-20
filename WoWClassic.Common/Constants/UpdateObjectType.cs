using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoWClassic.Common.Constants
{
    public enum UpdateObjectType : byte
    {
        Partial = 0,
        Movement,
        Full,
        CreateObject,
        OutOfRange,
        InRange,
    }
}
