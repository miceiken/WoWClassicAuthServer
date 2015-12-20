using System;
using LinqToDB.Mapping;
using WoWClassic.Datastore.Login;

namespace WoWClassic.Datastore.Gateway
{
    [Table(Name = "Characters")]
    public class Character
    {
        [PrimaryKey, Identity]
        public ulong CharacterID { get; set; }

        [Column]
        public int AccountID { get; set; }

        [Column]
        public string Name { get; set; }

        [Column]
        public byte Race { get; set; }

        [Column]
        public byte Class { get; set; }

        [Column]
        public byte Gender { get; set; }

        [Column]
        public byte Skin { get; set; }

        [Column]
        public byte Face { get; set; }

        [Column]
        public byte HairStyle { get; set; }

        [Column]
        public byte HairColor { get; set; }

        [Column]
        public byte FacialHair { get; set; }

        [Column]
        public byte Level { get; set; }

        [Column]
        public uint Zone { get; set; }

        [Column]
        public uint Map { get; set; }

        [Column]
        public float X { get; set; }

        [Column]
        public float Y { get; set; }

        [Column]
        public float Z { get; set; }

        [Column]
        public uint GuildId { get; set; }

        [Column]
        public uint CharacterFlags { get; set; }

        [Column]
        public bool FirstLogin { get; set; }

        [Association(ThisKey = "AccountID", OtherKey = "AccountID", CanBeNull = false)]
        public Account Account { get; set; }
    }
}
