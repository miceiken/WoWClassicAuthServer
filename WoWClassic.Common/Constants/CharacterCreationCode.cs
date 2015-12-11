using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoWClassic.Common.Constants
{
    public enum CharacterCreationCode : byte
    {
        InProgress = 0x2D,
        Success,
        Error,
        Failed,
        NameInUse,
        Disabled,
        PvPViolation,
        ServerLimit,
        AccountLimit,
        ServerQueue,
        OnlyExisting
    }
}
