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

        public override byte[] GetDescriptorFieldFor(Enum descriptor, Player target)
        {
            switch ((GameObjectFields)descriptor)
            {
                case GameObjectFields.OBJECT_FIELD_CREATED_BY:
                    break;
                case GameObjectFields.GAMEOBJECT_DISPLAYID:
                    break;
                case GameObjectFields.GAMEOBJECT_FLAGS:
                    break;
                case GameObjectFields.GAMEOBJECT_ROTATION:
                    break;
                case GameObjectFields.GAMEOBJECT_STATE:
                    break;
                case GameObjectFields.GAMEOBJECT_POS_X:
                    break;
                case GameObjectFields.GAMEOBJECT_POS_Y:
                    break;
                case GameObjectFields.GAMEOBJECT_POS_Z:
                    break;
                case GameObjectFields.GAMEOBJECT_FACING:
                    break;
                case GameObjectFields.GAMEOBJECT_DYN_FLAGS:
                    break;
                case GameObjectFields.GAMEOBJECT_FACTION:
                    break;
                case GameObjectFields.GAMEOBJECT_TYPE_ID:
                    break;
                case GameObjectFields.GAMEOBJECT_LEVEL:
                    break;
                case GameObjectFields.GAMEOBJECT_ARTKIT:
                    break;
                case GameObjectFields.GAMEOBJECT_ANIMPROGRESS:
                    break;
                case GameObjectFields.GAMEOBJECT_PADDING:
                    break;
            }
            return base.GetDescriptorFieldFor(descriptor, target);
        }
    }
}
