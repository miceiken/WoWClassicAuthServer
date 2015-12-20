using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoWClassic.Common.Constants
{
    public enum CharacterDeleteCode : byte
    {
        InProgress = 0x38,
        Success,
        Failed,
    }
}
