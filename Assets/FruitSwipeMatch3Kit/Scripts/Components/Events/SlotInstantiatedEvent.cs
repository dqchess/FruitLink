using Unity.Entities;

namespace FruitSwipeMatch3Kit
{
    public struct SlotInstantiatedEvent : IComponentData
    {
        public SlotType Type;
    }
}