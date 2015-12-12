using System;
using WoWClassic.Common.Packets;

namespace WoWClassic.Common.DataStructure
{
    public class Realm
    {
        public RealmType Type;
        public RealmFlags Flags;
        [PacketString(StringTypes.CString)]
        public string Name;
        [PacketString(StringTypes.CString)]
        public string Address;
        public float Population; // PlayerCount / MaxPlayerCount * 2
        public byte Characters;
        public RealmTimezone Timezone;
        private byte unk0 = 0;
    }

    public enum RealmType : uint
    {
        Normal = 0,
        PvP,
        RP = 6,
        RPPvP = 8,
    };

    [Flags]
    public enum RealmFlags : byte
    {
        None = 0,
        Offline = 2,
        SpecifyVersion = 4, // ?
        NewPlayers = 32,
        Recommended = 64,
    };

    public static class RealmPopulationPreset
    {
        public const float Low = 0.5f;
        public const float Medium = 1.0f;
        public const float High = 2.0f;
    }

    // If timezone doesn't match client's locale it won't show up!
    public enum RealmTimezone : byte
    {
        AnyLocale = 0,
        UnitedStates = 1,
        Korea = 2,
        English = 3,
        Taiwan = 4,
        China = 5,
        TestServer = 99,
        QAServer = 101,
    };
}
