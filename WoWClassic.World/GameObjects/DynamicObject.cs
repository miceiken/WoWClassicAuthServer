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
    public class DynamicObject : BaseObject
    {
        private static uint s_GlobalDynamicObjectIdentifier = 0;

        public DynamicObject(WoWGUID guid)
            : base(guid)
        { }

        public DynamicObject(uint ident)
            : this(new WoWGUID(HighGUIDType.DynamicObject, ident))
        { }

        public DynamicObject()
            : this(s_GlobalDynamicObjectIdentifier++)
        { }

        public override ObjectType Type => ObjectType.DynamicObject;

        public override byte[] GetDescriptorFieldFor(Enum descriptor, Player target)
        {
            switch ((DynamicObjectFields)descriptor)
            {
                case DynamicObjectFields.DYNAMICOBJECT_CASTER:
                    break;
                case DynamicObjectFields.DYNAMICOBJECT_BYTES:
                    break;
                case DynamicObjectFields.DYNAMICOBJECT_SPELLID:
                    break;
                case DynamicObjectFields.DYNAMICOBJECT_RADIUS:
                    break;
                case DynamicObjectFields.DYNAMICOBJECT_POS_X:
                    break;
                case DynamicObjectFields.DYNAMICOBJECT_POS_Y:
                    break;
                case DynamicObjectFields.DYNAMICOBJECT_POS_Z:
                    break;
                case DynamicObjectFields.DYNAMICOBJECT_FACING:
                    break;
                case DynamicObjectFields.DYNAMICOBJECT_PAD:
                    break;
            }
            return base.GetDescriptorFieldFor(descriptor, target);
        }
    }
}
