using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoWClassicServer.AuthServer.Constants
{
    public struct RealmInfo
    {
        public RealmFlags Flags;
        public string Name;
        public string Address;
        public float Population; // PlayerCount / MaxPlayerCount * 2
        public byte Characters;
        public RealmTimezone Timezone;
    }

    [Flags]
    public enum RealmFlags : byte
    {
        Offline = 2,
        SupportedVersion = 4, // ?
        NewPlayers = 32,
        Recommended = 64,
    };

    public enum RealmType : uint
    {
        Normal = 0,
        PvP,
        RP = 6,
        RPPvP = 8,
    };
    
    public static class RealmPopulationPreset
    {
        public const float Low = 0.5f;
        public const float Medium = 1.0f;
        public const float High = 2.0f;
    }

    public enum RealmTimezone : byte
    {
        UnitedStates = 1,
        Korea = 2,
        English = 3,
        Taiwan = 4,
        China = 5,
        TestServer = 99,
        QAServer = 101,
    };
}
