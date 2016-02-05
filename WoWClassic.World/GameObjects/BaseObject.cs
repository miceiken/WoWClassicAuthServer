using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWClassic.Common;
using WoWClassic.Common.Constants;
using WoWClassic.Common.DataStructure;
using WoWClassic.Common.Constants.Game;

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
        public uint Entry { get; set; } = 0;

        public virtual byte[] GetDescriptorFieldFor(Enum descriptor, Player target)
        {
            switch ((ObjectFields)descriptor)
            {
                case ObjectFields.OBJECT_FIELD_GUID:
                    return BitConverter.GetBytes(GUID.GUID);
                case ObjectFields.OBJECT_FIELD_TYPE:
                    return BitConverter.GetBytes((uint)Type);
                case ObjectFields.OBJECT_FIELD_ENTRY:
                    return BitConverter.GetBytes(Entry);
                case ObjectFields.OBJECT_FIELD_SCALE_X:
                    break;
                case ObjectFields.OBJECT_FIELD_PADDING:
                    break;
            }
            return null;
        }

        public virtual byte[] BuildValueUpdate(Player target)
        { // TOOD: your shit
            return null;
        }
    }
}
