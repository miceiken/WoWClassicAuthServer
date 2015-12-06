using System;
using System.IO;
using System.Text;

namespace WoWClassic.Common.Constants
{
    public struct RealmInfo
    {
        public RealmType Type;
        public RealmFlags Flags;
        public string Name;
        public string Address;
        public float Population; // PlayerCount / MaxPlayerCount * 2
        public byte Characters;
        public RealmTimezone Timezone;

        public static RealmInfo Read(BinaryReader br)
        {
            var r = new RealmInfo();
            r.Type = (RealmType)br.ReadUInt32();
            r.Flags = (RealmFlags)br.ReadByte();
            r.Name = br.ReadCString();
            r.Address = br.ReadCString();
            r.Population = br.ReadSingle();
            r.Characters = br.ReadByte();
            r.Timezone = (RealmTimezone)br.ReadByte();
            return r;
        }

        public byte[] ToByteArray()
        {
            using (var ms = new MemoryStream())
            using (var bw = new GenericWriter(ms))
            {
                bw.Write(Type);
                bw.Write(Flags);
                bw.Write(Encoding.ASCII.GetBytes(Name + '\0'));
                bw.Write(Encoding.ASCII.GetBytes(Address + '\0'));
                bw.Write(Population);
                bw.Write(Characters);
                bw.Write(Timezone);
                bw.Write<byte>(0);

                return ms.ToArray();
            }
        }
    }

    [Flags]
    public enum RealmFlags : byte
    {
        None = 0,
        Offline = 2,
        SpecifyVersion = 4, // ?
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
