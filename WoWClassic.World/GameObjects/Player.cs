using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWClassic.Common.Constants;
using WoWClassic.Common.DataStructure;

namespace WoWClassic.World.GameObjects
{
    public class Player : Unit
    {
        public Player(WoWGUID guid)
            : base(guid)
        {
            CharacterID = guid.BackendGUID;
        }

        public Player(uint ident)
            : this(new WoWGUID(HighGUIDType.Player, ident))
        { }

        public override ObjectType Type => ObjectType.Player;

        public uint CharacterID
        {
            get;
            private set;
        }

        
    }
}
