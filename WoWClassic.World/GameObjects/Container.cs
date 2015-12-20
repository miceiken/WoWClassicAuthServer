using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWClassic.Common.Constants;
using WoWClassic.Common.DataStructure;

namespace WoWClassic.World.GameObjects
{
    public class Container : Item
    {
        private static uint s_GlobalContainerIdentifier = 0;

        public Container(WoWGUID guid)
            : base(guid)
        { }

        public Container(uint ident)
            : this(new WoWGUID(HighGUIDType.Container, ident))
        { }

        public Container()
            : this(s_GlobalContainerIdentifier++)
        { }

        public override ObjectType Type => ObjectType.Container;
    }
}
