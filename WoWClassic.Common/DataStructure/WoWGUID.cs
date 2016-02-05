using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWClassic.Common;
using WoWClassic.Common.Constants;

namespace WoWClassic.Common.DataStructure
{
    public class WoWGUID
    {
        public WoWGUID(ulong guid)
        {
            GUID = guid;
        }

        public WoWGUID(uint high, uint low)
            : this((high << 32) & low)
        { }

        public WoWGUID(HighGUIDType high, uint low)
            : this((uint)high, low)
        { }

        public ulong GUID { get; private set; }
        public bool IsValid => GUID > 0;

        public uint BackendGUID => (uint)(GUID >> 32);

        private uint ObjectTypeDescriptor => (uint)(GUID >> 48) & 0xFFFF;
        private HighGUIDType HighGUIDType => (HighGUIDType)ObjectTypeDescriptor;
        public bool IsItem => HighGUIDType == HighGUIDType.Item;
        public bool IsPlayer => HighGUIDType == HighGUIDType.Player;
        public bool IsGameObject => HighGUIDType == HighGUIDType.GameObject;
        public bool IsUnit => HighGUIDType == HighGUIDType.Unit;
        public bool IsPet => HighGUIDType == HighGUIDType.Pet;
        public bool IsDynamicObject => HighGUIDType == HighGUIDType.DynamicObject;
        public bool IsCorpse => HighGUIDType == HighGUIDType.Corpse;
        public bool IsTransport => HighGUIDType == HighGUIDType.Transport;
        public bool IsMapObjectTransport => HighGUIDType == HighGUIDType.MapObjectTransport;

        public bool HasEntry => IsGameObject || IsTransport || IsUnit || IsPet;
        public uint Entry => HasEntry ? (uint)((GUID >> 48) & 0xFFFFFF) : 0;

        public ObjectType ObjectType => s_GUIDObjectMap[HighGUIDType];

        public byte[] ToPacked()
        {
            var guid = GUID;
            var packed = new List<byte>();
            byte c = 0;
            for (byte i = 0; guid > 0; i++)
            {
                if ((guid & 0xFF) > 0)
                {
                    packed[0] |= (byte)(1 << i);
                    packed[c++] = (byte)(guid & 0xFF);
                }
                guid >>= 8;
            }
            return packed.GetRange(0, c).ToArray();
        }

        private static readonly Dictionary<HighGUIDType, ObjectType> s_GUIDObjectMap = new Dictionary<HighGUIDType, ObjectType>()
        {
            [HighGUIDType.Item] = ObjectType.Item,
            [HighGUIDType.Player] = ObjectType.Player,
            [HighGUIDType.GameObject] = ObjectType.GameObject,
            [HighGUIDType.Unit] = ObjectType.Unit,
            [HighGUIDType.Pet] = ObjectType.Unit,
            [HighGUIDType.DynamicObject] = ObjectType.DynamicObject,
            [HighGUIDType.Corpse] = ObjectType.Corpse,
            [HighGUIDType.Transport] = ObjectType.GameObject, // ?
            [HighGUIDType.MapObjectTransport] = ObjectType.GameObject
        };
    }
}
