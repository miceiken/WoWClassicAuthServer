using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoWClassic.Common.Constants
{
    public enum ObjectType : byte
    {
        Object = 0,
        Item,
        Container,
        Unit,
        Player,
        GameObject,
        DynamicObject,
        Corpse
    }

    [Flags]
    public enum ObjectTypeMask : byte
    {
        None = 0,

        Object = 1 << ObjectType.Object,
        Item = 1 << ObjectType.Item,
        Container = 1 << ObjectType.Container,
        Unit = 1 << ObjectType.Unit,
        Player = 1 << ObjectType.Player,
        GameObject = 1 << ObjectType.GameObject,
        DynamicObject = 1 << ObjectType.DynamicObject,
        Corpse = 1 << ObjectType.Corpse,
    }
}
