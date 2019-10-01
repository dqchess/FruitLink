// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store EULA,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;

namespace FruitSwipeMatch3Kit
{
    /// <summary>
    /// Stores the data of a level goal.
    /// </summary>
    [Serializable]
    public class LevelGoalData
    {
        public GoalType Type;

        public ColorTileType ColorTileType;
        public RandomColorTileType RandomColorTileType;
        public SlotType SlotType;
        public BlockerType BlockerType;
        public CollectibleType CollectibleType;
        
        public int Amount;

        public override string ToString()
        {
            switch (Type)
            {
                case GoalType.CollectTiles:
                    return $"Collect {Amount} tiles";
                
                case GoalType.CollectRandomTiles:
                    return $"Collect {Amount} random tiles";
                
                case GoalType.CollectSlots:
                    return $"Collect {Amount} slots";
                
                case GoalType.CollectBlockers:
                    return $"Collect {Amount} blockers";
                
                case GoalType.CollectCollectibles:
                    return $"Collect {Amount} collectibles";
                
                default:
                    return string.Empty;
            }
        }
    }
}
