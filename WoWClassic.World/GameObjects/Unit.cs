using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWClassic.Common.Constants;
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
    }
}
