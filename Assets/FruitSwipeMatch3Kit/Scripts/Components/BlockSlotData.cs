using System;
using Unity.Entities;

namespace FruitSwipeMatch3Kit
{
    [Serializable]
    public struct BlockSlotData : IComponentData
    {
        public int tilePosition;
    }
}