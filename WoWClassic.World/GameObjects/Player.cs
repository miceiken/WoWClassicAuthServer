using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWClassic.Common.Constants;
using WoWClassic.Common.Constants.Game;
using WoWClassic.Common.DataStructure;

namespace WoWClassic.World.GameObjects
{
    public class Player : Unit
    {
        public Player(WoWGUID guid)
            : base(guid)
        {
            CharacterID = guid.BackendGUID;
        }

        public Player(uint ident)
            : this(new WoWGUID(HighGUIDType.Player, ident))
        { }

        public override ObjectType Type => ObjectType.Player;

        public uint CharacterID
        {
            get;
            private set;
        }

        public uint Experience { get; set } = 0;
        public uint RestedExperience { get; set; } = 0;
        public uint NextLevelExperience { get; set; } = 999;

        public float BlockChance { get; set; } = 4.0f;
        public float DodgeChance { get; set; } = 4.0f;
        public float ParryChance { get; set; } = 4.0f;
        public float CriticalHitChance { get; set; } = 4.0f;
        public float RangedCriticalHitChance { get; set; } = 4.0f;

        public uint Coinage { get; set; } = 0;


        public override byte[] GetDescriptorFieldFor(Enum descriptor, Player target)
        {
            switch ((PlayerFields)descriptor)
            {
                case PlayerFields.PLAYER_DUEL_ARBITER:
                    break;
                case PlayerFields.PLAYER_FLAGS:
                    break;
                case PlayerFields.PLAYER_GUILDID:
                    break;
                case PlayerFields.PLAYER_GUILDRANK:
                    break;
                case PlayerFields.PLAYER_BYTES:
                    break;
                case PlayerFields.PLAYER_BYTES_2:
                    break;
                case PlayerFields.PLAYER_BYTES_3:
                    break;
                case PlayerFields.PLAYER_DUEL_TEAM:
                    break;
                case PlayerFields.PLAYER_GUILD_TIMESTAMP:
                    break;
                case PlayerFields.PLAYER_QUEST_LOG_1_1:
                    break;
                case PlayerFields.PLAYER_QUEST_LOG_1_2:
                    break;
                case PlayerFields.PLAYER_QUEST_LOG_1_3:
                    break;
                case PlayerFields.PLAYER_QUEST_LOG_LAST_1:
                    break;
                case PlayerFields.PLAYER_QUEST_LOG_LAST_2:
                    break;
                case PlayerFields.PLAYER_QUEST_LOG_LAST_3:
                    break;
                case PlayerFields.PLAYER_VISIBLE_ITEM_1_CREATOR:
                    break;
                case PlayerFields.PLAYER_VISIBLE_ITEM_1_0:
                    break;
                case PlayerFields.PLAYER_VISIBLE_ITEM_1_PROPERTIES:
                    break;
                case PlayerFields.PLAYER_VISIBLE_ITEM_1_PAD:
                    break;
                case PlayerFields.PLAYER_VISIBLE_ITEM_LAST_CREATOR:
                    break;
                case PlayerFields.PLAYER_VISIBLE_ITEM_LAST_0:
                    break;
                case PlayerFields.PLAYER_VISIBLE_ITEM_LAST_PROPERTIES:
                    break;
                case PlayerFields.PLAYER_VISIBLE_ITEM_LAST_PAD:
                    break;
                case PlayerFields.PLAYER_FIELD_INV_SLOT_HEAD:
                    break;
                case PlayerFields.PLAYER_FIELD_PACK_SLOT_1:
                    break;
                case PlayerFields.PLAYER_FIELD_PACK_SLOT_LAST:
                    break;
                case PlayerFields.PLAYER_FIELD_BANK_SLOT_1:
                    break;
                case PlayerFields.PLAYER_FIELD_BANK_SLOT_LAST:
                    break;
                case PlayerFields.PLAYER_FIELD_BANKBAG_SLOT_1:
                    break;
                case PlayerFields.PLAYER_FIELD_BANKBAG_SLOT_LAST:
                    break;
                case PlayerFields.PLAYER_FIELD_VENDORBUYBACK_SLOT_1:
                    break;
                case PlayerFields.PLAYER_FIELD_VENDORBUYBACK_SLOT_LAST:
                    break;
                case PlayerFields.PLAYER_FIELD_KEYRING_SLOT_1:
                    break;
                case PlayerFields.PLAYER_FIELD_KEYRING_SLOT_LAST:
                    break;
                case PlayerFields.PLAYER_FARSIGHT:
                    break;
                case PlayerFields.PLAYER_FIELD_COMBO_TARGET:
                    break;
                case PlayerFields.PLAYER_XP:
                    break;
                case PlayerFields.PLAYER_NEXT_LEVEL_XP:
                    break;
                case PlayerFields.PLAYER_SKILL_INFO_1_1:
                    break;
                case PlayerFields.PLAYER_CHARACTER_POINTS1:
                    break;
                case PlayerFields.PLAYER_CHARACTER_POINTS2:
                    break;
                case PlayerFields.PLAYER_TRACK_CREATURES:
                    break;
                case PlayerFields.PLAYER_TRACK_RESOURCES:
                    break;
                case PlayerFields.PLAYER_BLOCK_PERCENTAGE:
                    break;
                case PlayerFields.PLAYER_DODGE_PERCENTAGE:
                    break;
                case PlayerFields.PLAYER_PARRY_PERCENTAGE:
                    break;
                case PlayerFields.PLAYER_CRIT_PERCENTAGE:
                    break;
                case PlayerFields.PLAYER_RANGED_CRIT_PERCENTAGE:
                    break;
                case PlayerFields.PLAYER_EXPLORED_ZONES_1:
                    break;
                case PlayerFields.PLAYER_REST_STATE_EXPERIENCE:
                    break;
                case PlayerFields.PLAYER_FIELD_COINAGE:
                    break;
                case PlayerFields.PLAYER_FIELD_POSSTAT0:
                    break;
                case PlayerFields.PLAYER_FIELD_POSSTAT1:
                    break;
                case PlayerFields.PLAYER_FIELD_POSSTAT2:
                    break;
                case PlayerFields.PLAYER_FIELD_POSSTAT3:
                    break;
                case PlayerFields.PLAYER_FIELD_POSSTAT4:
                    break;
                case PlayerFields.PLAYER_FIELD_NEGSTAT0:
                    break;
                case PlayerFields.PLAYER_FIELD_NEGSTAT1:
                    break;
                case PlayerFields.PLAYER_FIELD_NEGSTAT2:
                    break;
                case PlayerFields.PLAYER_FIELD_NEGSTAT3:
                    break;
                case PlayerFields.PLAYER_FIELD_NEGSTAT4:
                    break;
                case PlayerFields.PLAYER_FIELD_RESISTANCEBUFFMODSPOSITIVE:
                    break;
                case PlayerFields.PLAYER_FIELD_RESISTANCEBUFFMODSNEGATIVE:
                    break;
                case PlayerFields.PLAYER_FIELD_MOD_DAMAGE_DONE_POS:
                    break;
                case PlayerFields.PLAYER_FIELD_MOD_DAMAGE_DONE_NEG:
                    break;
                case PlayerFields.PLAYER_FIELD_MOD_DAMAGE_DONE_PCT:
                    break;
                case PlayerFields.PLAYER_FIELD_BYTES:
                    break;
                case PlayerFields.PLAYER_AMMO_ID:
                    break;
                case PlayerFields.PLAYER_SELF_RES_SPELL:
                    break;
                case PlayerFields.PLAYER_FIELD_PVP_MEDALS:
                    break;
                case PlayerFields.PLAYER_FIELD_BUYBACK_PRICE_1:
                    break;
                case PlayerFields.PLAYER_FIELD_BUYBACK_PRICE_LAST:
                    break;
                case PlayerFields.PLAYER_FIELD_BUYBACK_TIMESTAMP_1:
                    break;
                case PlayerFields.PLAYER_FIELD_BUYBACK_TIMESTAMP_LAST:
                    break;
                case PlayerFields.PLAYER_FIELD_SESSION_KILLS:
                    break;
                case PlayerFields.PLAYER_FIELD_YESTERDAY_KILLS:
                    break;
                case PlayerFields.PLAYER_FIELD_LAST_WEEK_KILLS:
                    break;
                case PlayerFields.PLAYER_FIELD_THIS_WEEK_KILLS:
                    break;
                case PlayerFields.PLAYER_FIELD_THIS_WEEK_CONTRIBUTION:
                    break;
                case PlayerFields.PLAYER_FIELD_LIFETIME_HONORABLE_KILLS:
                    break;
                case PlayerFields.PLAYER_FIELD_LIFETIME_DISHONORABLE_KILLS:
                    break;
                case PlayerFields.PLAYER_FIELD_YESTERDAY_CONTRIBUTION:
                    break;
                case PlayerFields.PLAYER_FIELD_LAST_WEEK_CONTRIBUTION:
                    break;
                case PlayerFields.PLAYER_FIELD_LAST_WEEK_RANK:
                    break;
                case PlayerFields.PLAYER_FIELD_BYTES2:
                    break;
                case PlayerFields.PLAYER_FIELD_WATCHED_FACTION_INDEX:
                    break;
                case PlayerFields.PLAYER_FIELD_COMBAT_RATING_1:
                    break;
            }
            return base.GetDescriptorFieldFor(descriptor, target);
        }

        private static readonly List<Enum> VisibleBits = new List<Enum>()
        {
            ObjectFields.OBJECT_FIELD_GUID,
            ObjectFields.OBJECT_FIELD_TYPE,
            ObjectFields.OBJECT_FIELD_SCALE_X,

            UnitFields.UNIT_FIELD_HEALTH,
            UnitFields.UNIT_FIELD_POWER1,
            UnitFields.UNIT_FIELD_POWER2,
            UnitFields.UNIT_FIELD_POWER3,
            UnitFields.UNIT_FIELD_POWER4,
            UnitFields.UNIT_FIELD_POWER5,
            UnitFields.UNIT_FIELD_MAXHEALTH,
            UnitFields.UNIT_FIELD_MAXPOWER1,
            UnitFields.UNIT_FIELD_MAXPOWER2,
            UnitFields.UNIT_FIELD_MAXPOWER3,
            UnitFields.UNIT_FIELD_MAXPOWER4,
            UnitFields.UNIT_FIELD_MAXPOWER5,
            UnitFields.UNIT_FIELD_LEVEL,
            UnitFields.UNIT_FIELD_FACTIONTEMPLATE,
            UnitFields.UNIT_FIELD_BYTES_0,
            UnitFields.UNIT_FIELD_BASEATTACKTIME,
            UnitFields.UNIT_FIELD_BASEATTACKTIME+1,
            UnitFields.UNIT_FIELD_BOUNDINGRADIUS,
            UnitFields.UNIT_FIELD_COMBATREACH,
            UnitFields.UNIT_FIELD_DISPLAYID,
            UnitFields.UNIT_FIELD_NATIVEDISPLAYID,
            UnitFields.UNIT_FIELD_MOUNTDISPLAYID,
            UnitFields.UNIT_FIELD_MINDAMAGE,
            UnitFields.UNIT_FIELD_MAXDAMAGE,
            UnitFields.UNIT_FIELD_MINOFFHANDDAMAGE,
            UnitFields.UNIT_FIELD_MAXOFFHANDDAMAGE,
            UnitFields.UNIT_FIELD_BYTES_1,
            UnitFields.UNIT_MOD_CAST_SPEED,
            // STATS
            // RESISTANCE
            UnitFields.UNIT_FIELD_BASE_MANA,
            UnitFields.UNIT_FIELD_BASE_HEALTH,
            UnitFields.UNIT_FIELD_BYTES_2,
            UnitFields.UNIT_FIELD_ATTACK_POWER,
            UnitFields.UNIT_FIELD_ATTACK_POWER_MODS,
            UnitFields.UNIT_FIELD_MINRANGEDDAMAGE,
            UnitFields.UNIT_FIELD_MAXRANGEDDAMAGE,

            PlayerFields.PLAYER_FLAGS,
            PlayerFields.PLAYER_BYTES,
            PlayerFields.PLAYER_BYTES_2,
            PlayerFields.PLAYER_BYTES_3,
            // VISIBLE ITEMS
            PlayerFields.PLAYER_XP,
            PlayerFields.PLAYER_NEXT_LEVEL_XP,
            // SKILLS
            PlayerFields.PLAYER_CHARACTER_POINTS1,
            PlayerFields.PLAYER_CHARACTER_POINTS2,
            PlayerFields.PLAYER_BLOCK_PERCENTAGE,
            PlayerFields.PLAYER_DODGE_PERCENTAGE,
            PlayerFields.PLAYER_PARRY_PERCENTAGE,
            PlayerFields.PLAYER_CRIT_PERCENTAGE,
            PlayerFields.PLAYER_RANGED_CRIT_PERCENTAGE,
            // EXPLORED ZONES
            PlayerFields.PLAYER_REST_STATE_EXPERIENCE,
            PlayerFields.PLAYER_FIELD_COINAGE,
            // STATS
            // RESISTANCE
            PlayerFields.PLAYER_FIELD_WATCHED_FACTION_INDEX,

        };

        public override byte[] BuildValueUpdate(Player target)
        {
            return base.BuildValueUpdate(target);
        }


    }
}
