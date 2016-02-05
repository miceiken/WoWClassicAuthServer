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
    public class Unit : BaseObject
    {
        private static uint s_GlobalUnitIdentifier = 0;

        public Unit(WoWGUID guid)
            : base(guid)
        { }

        public Unit(uint ident)
            : this(new WoWGUID(HighGUIDType.Unit, ident))
        { }

        public Unit()
            : this(s_GlobalUnitIdentifier++)
        { }

        public override ObjectType Type => ObjectType.Unit;

        // TODO: Fix default values

        public uint Health { get; set; } = 10;
        public uint MaxHealth { get; set; } = 10;

        #region Power

        public uint Mana { get; set; } = 10;
        public uint MaxMana { get; set; } = 10;

        public uint Rage { get; set; } = 10;
        public uint MaxRage { get; set; } = 10;

        public uint Focus { get; set; } = 10;
        public uint MaxFocus { get; set; } = 10;

        public uint Energy { get; set; } = 10;
        public uint MaxEnergy { get; set; } = 10;

        public uint Happiness { get; set; } = 10;
        public uint MaxHappiness { get; set; } = 10;

        #endregion

        public uint Level { get; set; } = 1;
        public uint FactionTemplate { get; set; }

        public WoWRace Race { get; set; }
        public WoWClass Class { get; set; }
        public WoWGender Gender { get; set; }
        public WoWPower Power { get; set; }
        public uint Bytes0
        {
            get { return (uint)((byte)Power << 24 | (byte)Gender << 16 | (byte)Class << 8 | (byte)Race); }
        }

        public override byte[] GetDescriptorFieldFor(Enum descriptor, Player target)
        {
            switch ((UnitFields)descriptor)
            {
                case UnitFields.UNIT_FIELD_CHARM:
                    break;
                case UnitFields.UNIT_FIELD_SUMMON:
                    break;
                case UnitFields.UNIT_FIELD_CHARMEDBY:
                    break;
                case UnitFields.UNIT_FIELD_SUMMONEDBY:
                    break;
                case UnitFields.UNIT_FIELD_CREATEDBY:
                    break;
                case UnitFields.UNIT_FIELD_TARGET:
                    break;
                case UnitFields.UNIT_FIELD_PERSUADED:
                    break;
                case UnitFields.UNIT_FIELD_CHANNEL_OBJECT:
                    break;
                case UnitFields.UNIT_FIELD_HEALTH:
                    break;
                case UnitFields.UNIT_FIELD_POWER1:
                    break;
                case UnitFields.UNIT_FIELD_POWER2:
                    break;
                case UnitFields.UNIT_FIELD_POWER3:
                    break;
                case UnitFields.UNIT_FIELD_POWER4:
                    break;
                case UnitFields.UNIT_FIELD_POWER5:
                    break;
                case UnitFields.UNIT_FIELD_MAXHEALTH:
                    break;
                case UnitFields.UNIT_FIELD_MAXPOWER1:
                    break;
                case UnitFields.UNIT_FIELD_MAXPOWER2:
                    break;
                case UnitFields.UNIT_FIELD_MAXPOWER3:
                    break;
                case UnitFields.UNIT_FIELD_MAXPOWER4:
                    break;
                case UnitFields.UNIT_FIELD_MAXPOWER5:
                    break;
                case UnitFields.UNIT_FIELD_LEVEL:
                    break;
                case UnitFields.UNIT_FIELD_FACTIONTEMPLATE:
                    break;
                case UnitFields.UNIT_FIELD_BYTES_0:
                    break;
                case UnitFields.UNIT_VIRTUAL_ITEM_SLOT_DISPLAY:
                    break;
                case UnitFields.UNIT_VIRTUAL_ITEM_SLOT_DISPLAY_01:
                    break;
                case UnitFields.UNIT_VIRTUAL_ITEM_SLOT_DISPLAY_02:
                    break;
                case UnitFields.UNIT_VIRTUAL_ITEM_INFO:
                    break;
                case UnitFields.UNIT_VIRTUAL_ITEM_INFO_01:
                    break;
                case UnitFields.UNIT_VIRTUAL_ITEM_INFO_02:
                    break;
                case UnitFields.UNIT_VIRTUAL_ITEM_INFO_03:
                    break;
                case UnitFields.UNIT_VIRTUAL_ITEM_INFO_04:
                    break;
                case UnitFields.UNIT_VIRTUAL_ITEM_INFO_05:
                    break;
                case UnitFields.UNIT_FIELD_FLAGS:
                    break;
                case UnitFields.UNIT_FIELD_AURA:
                    break;
                case UnitFields.UNIT_FIELD_AURA_LAST:
                    break;
                case UnitFields.UNIT_FIELD_AURAFLAGS:
                    break;
                case UnitFields.UNIT_FIELD_AURAFLAGS_01:
                    break;
                case UnitFields.UNIT_FIELD_AURAFLAGS_02:
                    break;
                case UnitFields.UNIT_FIELD_AURAFLAGS_03:
                    break;
                case UnitFields.UNIT_FIELD_AURAFLAGS_04:
                    break;
                case UnitFields.UNIT_FIELD_AURAFLAGS_05:
                    break;
                case UnitFields.UNIT_FIELD_AURALEVELS:
                    break;
                case UnitFields.UNIT_FIELD_AURALEVELS_LAST:
                    break;
                case UnitFields.UNIT_FIELD_AURAAPPLICATIONS:
                    break;
                case UnitFields.UNIT_FIELD_AURAAPPLICATIONS_LAST:
                    break;
                case UnitFields.UNIT_FIELD_AURASTATE:
                    break;
                case UnitFields.UNIT_FIELD_BASEATTACKTIME:
                    break;
                case UnitFields.UNIT_FIELD_OFFHANDATTACKTIME:
                    break;
                case UnitFields.UNIT_FIELD_RANGEDATTACKTIME:
                    break;
                case UnitFields.UNIT_FIELD_BOUNDINGRADIUS:
                    break;
                case UnitFields.UNIT_FIELD_COMBATREACH:
                    break;
                case UnitFields.UNIT_FIELD_DISPLAYID:
                    break;
                case UnitFields.UNIT_FIELD_NATIVEDISPLAYID:
                    break;
                case UnitFields.UNIT_FIELD_MOUNTDISPLAYID:
                    break;
                case UnitFields.UNIT_FIELD_MINDAMAGE:
                    break;
                case UnitFields.UNIT_FIELD_MAXDAMAGE:
                    break;
                case UnitFields.UNIT_FIELD_MINOFFHANDDAMAGE:
                    break;
                case UnitFields.UNIT_FIELD_MAXOFFHANDDAMAGE:
                    break;
                case UnitFields.UNIT_FIELD_BYTES_1:
                    break;
                case UnitFields.UNIT_FIELD_PETNUMBER:
                    break;
                case UnitFields.UNIT_FIELD_PET_NAME_TIMESTAMP:
                    break;
                case UnitFields.UNIT_FIELD_PETEXPERIENCE:
                    break;
                case UnitFields.UNIT_FIELD_PETNEXTLEVELEXP:
                    break;
                case UnitFields.UNIT_DYNAMIC_FLAGS:
                    break;
                case UnitFields.UNIT_CHANNEL_SPELL:
                    break;
                case UnitFields.UNIT_MOD_CAST_SPEED:
                    break;
                case UnitFields.UNIT_CREATED_BY_SPELL:
                    break;
                case UnitFields.UNIT_NPC_FLAGS:
                    break;
                case UnitFields.UNIT_NPC_EMOTESTATE:
                    break;
                case UnitFields.UNIT_TRAINING_POINTS:
                    break;
                case UnitFields.UNIT_FIELD_STAT0:
                    break;
                case UnitFields.UNIT_FIELD_STAT1:
                    break;
                case UnitFields.UNIT_FIELD_STAT2:
                    break;
                case UnitFields.UNIT_FIELD_STAT3:
                    break;
                case UnitFields.UNIT_FIELD_STAT4:
                    break;
                case UnitFields.UNIT_FIELD_RESISTANCES:
                    break;
                case UnitFields.UNIT_FIELD_RESISTANCES_01:
                    break;
                case UnitFields.UNIT_FIELD_RESISTANCES_02:
                    break;
                case UnitFields.UNIT_FIELD_RESISTANCES_03:
                    break;
                case UnitFields.UNIT_FIELD_RESISTANCES_04:
                    break;
                case UnitFields.UNIT_FIELD_RESISTANCES_05:
                    break;
                case UnitFields.UNIT_FIELD_RESISTANCES_06:
                    break;
                case UnitFields.UNIT_FIELD_BASE_MANA:
                    break;
                case UnitFields.UNIT_FIELD_BASE_HEALTH:
                    break;
                case UnitFields.UNIT_FIELD_BYTES_2:
                    break;
                case UnitFields.UNIT_FIELD_ATTACK_POWER:
                    break;
                case UnitFields.UNIT_FIELD_ATTACK_POWER_MODS:
                    break;
                case UnitFields.UNIT_FIELD_ATTACK_POWER_MULTIPLIER:
                    break;
                case UnitFields.UNIT_FIELD_RANGED_ATTACK_POWER:
                    break;
                case UnitFields.UNIT_FIELD_RANGED_ATTACK_POWER_MODS:
                    break;
                case UnitFields.UNIT_FIELD_RANGED_ATTACK_POWER_MULTIPLIER:
                    break;
                case UnitFields.UNIT_FIELD_MINRANGEDDAMAGE:
                    break;
                case UnitFields.UNIT_FIELD_MAXRANGEDDAMAGE:
                    break;
                case UnitFields.UNIT_FIELD_POWER_COST_MODIFIER:
                    break;
                case UnitFields.UNIT_FIELD_POWER_COST_MODIFIER_01:
                    break;
                case UnitFields.UNIT_FIELD_POWER_COST_MODIFIER_02:
                    break;
                case UnitFields.UNIT_FIELD_POWER_COST_MODIFIER_03:
                    break;
                case UnitFields.UNIT_FIELD_POWER_COST_MODIFIER_04:
                    break;
                case UnitFields.UNIT_FIELD_POWER_COST_MODIFIER_05:
                    break;
                case UnitFields.UNIT_FIELD_POWER_COST_MODIFIER_06:
                    break;
                case UnitFields.UNIT_FIELD_POWER_COST_MULTIPLIER:
                    break;
                case UnitFields.UNIT_FIELD_POWER_COST_MULTIPLIER_01:
                    break;
                case UnitFields.UNIT_FIELD_POWER_COST_MULTIPLIER_02:
                    break;
                case UnitFields.UNIT_FIELD_POWER_COST_MULTIPLIER_03:
                    break;
                case UnitFields.UNIT_FIELD_POWER_COST_MULTIPLIER_04:
                    break;
                case UnitFields.UNIT_FIELD_POWER_COST_MULTIPLIER_05:
                    break;
                case UnitFields.UNIT_FIELD_POWER_COST_MULTIPLIER_06:
                    break;
                case UnitFields.UNIT_FIELD_PADDING:
                    break;
            }
            return base.GetDescriptorFieldFor(descriptor, target);
        }
    }
}
