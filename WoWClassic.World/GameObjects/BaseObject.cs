using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWClassic.Common;
using WoWClassic.Common.Constants;
using WoWClassic.Common.DataStructure;

namespace WoWClassic.World.GameObjects
{
    public class BaseObject
    {
        public BaseObject(WoWGUID guid)
        {
            if (!guid.IsValid)
                throw new Exception("Invalid GUID passed to BaseObject");

            GUID = guid;
        }

        public WoWGUID GUID { get; private set; }

        public virtual ObjectType Type => ObjectType.Object;
    }
}
