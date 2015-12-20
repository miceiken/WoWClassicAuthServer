using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWClassic.Common.Constants;
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
    }
}
