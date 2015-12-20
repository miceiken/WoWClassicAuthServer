using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWClassic.Common.Constants;
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
    }
}
