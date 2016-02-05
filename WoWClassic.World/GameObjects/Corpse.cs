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
    public class Corpse : BaseObject
    {
        private static uint s_GlobalCorpseIdentifier = 0;

        public Corpse(WoWGUID guid)
            : base(guid)
        { }

        public Corpse(uint ident)
            : this(new WoWGUID(HighGUIDType.Corpse, ident))
        { }

        public Corpse()
            : this(s_GlobalCorpseIdentifier++)
        { }

        public override ObjectType Type => ObjectType.Corpse;

        public override byte[] GetDescriptorFieldFor(Enum descriptor, Player target)
        {
            switch ((CorpseFields)descriptor)
            {
                case CorpseFields.CORPSE_FIELD_OWNER:
                    break;
                case CorpseFields.CORPSE_FIELD_FACING:
                    break;
                case CorpseFields.CORPSE_FIELD_POS_X:
                    break;
                case CorpseFields.CORPSE_FIELD_POS_Y:
                    break;
                case CorpseFields.CORPSE_FIELD_POS_Z:
                    break;
                case CorpseFields.CORPSE_FIELD_DISPLAY_ID:
                    break;
                case CorpseFields.CORPSE_FIELD_ITEM:
                    break;
                case CorpseFields.CORPSE_FIELD_BYTES_1:
                    break;
                case CorpseFields.CORPSE_FIELD_BYTES_2:
                    break;
                case CorpseFields.CORPSE_FIELD_GUILD:
                    break;
                case CorpseFields.CORPSE_FIELD_FLAGS:
                    break;
                case CorpseFields.CORPSE_FIELD_DYNAMIC_FLAGS:
                    break;
                case CorpseFields.CORPSE_FIELD_PAD:
                    break;
            }
            return base.GetDescriptorFieldFor(descriptor, target);
        }
    }
}
