using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWClassic.Common.Constants;
using WoWClassic.Common.DataStructure;

namespace WoWClassic.World.GameObjects
{
    public class GameObject : BaseObject
    {
        private static uint s_GlobalGameObjectIdentifier = 0;

        public GameObject(WoWGUID guid)
            : base(guid)
        { }

        public GameObject(uint ident)
            : this(new WoWGUID(HighGUIDType.GameObject, ident))
        { }

        public GameObject()
            : this(s_GlobalGameObjectIdentifier++)
        { }

        public override ObjectType Type => ObjectType.GameObject;
    }
}
