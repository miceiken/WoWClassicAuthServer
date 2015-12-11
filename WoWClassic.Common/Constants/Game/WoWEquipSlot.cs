using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoWClassic.Common.Constants.Game
{
    public enum WoWEquipSlot : byte
    {
        //None = -1,
        Head = 0,
        Neck,
        Shoulders,
        Body,
        Chest,
        Waist,
        Legs,
        Feet,
        Wrists,
        Hands,
        Finger1,
        Finger2,
        Trinket1,
        Trinket2,
        Back,
        MainHand,
        OffHand,
        Ranged,
        Tabard,
        Bag1,
        Bag2,
        Bag3,
        Bag4,

        Start = Head,
        End = Bag4,
    }
}
