using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoWClassic.Common.Constants
{
    public enum AuthState
    {
        LoginChallenge,
        ReconnectChallenge,
        LoginProof,
        ReconnectProof,
        RealmlistRequest,
    }
}
