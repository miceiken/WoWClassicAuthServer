using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWClassic.Common.Constants;
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
    }
}
