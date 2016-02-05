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
    public class Item : BaseObject
    {
        private static uint s_GlobalItemIdentifier = 0;

        public Item(WoWGUID guid)
            : base(guid)
        { }

        public Item(uint ident)
            : this(new WoWGUID(HighGUIDType.Item, ident))
        { }

        public Item()
            : this(s_GlobalItemIdentifier++)
        { }

        public override ObjectType Type => ObjectType.Item;

        public override byte[] GetDescriptorFieldFor(Enum descriptor, Player target)
        {
            switch ((ItemFields)descriptor)
            {
                case ItemFields.ITEM_FIELD_OWNER:
                    break;
                case ItemFields.ITEM_FIELD_CONTAINED:
                    break;
                case ItemFields.ITEM_FIELD_CREATOR:
                    break;
                case ItemFields.ITEM_FIELD_GIFTCREATOR:
                    break;
                case ItemFields.ITEM_FIELD_STACK_COUNT:
                    break;
                case ItemFields.ITEM_FIELD_DURATION:
                    break;
                case ItemFields.ITEM_FIELD_SPELL_CHARGES:
                    break;
                case ItemFields.ITEM_FIELD_SPELL_CHARGES_01:
                    break;
                case ItemFields.ITEM_FIELD_SPELL_CHARGES_02:
                    break;
                case ItemFields.ITEM_FIELD_SPELL_CHARGES_03:
                    break;
                case ItemFields.ITEM_FIELD_SPELL_CHARGES_04:
                    break;
                case ItemFields.ITEM_FIELD_FLAGS:
                    break;
                case ItemFields.ITEM_FIELD_ENCHANTMENT:
                    break;
                case ItemFields.ITEM_FIELD_PROPERTY_SEED:
                    break;
                case ItemFields.ITEM_FIELD_RANDOM_PROPERTIES_ID:
                    break;
                case ItemFields.ITEM_FIELD_ITEM_TEXT_ID:
                    break;
                case ItemFields.ITEM_FIELD_DURABILITY:
                    break;
                case ItemFields.ITEM_FIELD_MAXDURABILITY:
                    break;
            }
            return base.GetDescriptorFieldFor(descriptor, target);
        }
    }
}
