// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store EULA,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;

namespace FruitSwipeMatch3Kit
{
    /// <summary>
    /// Stores the data of a level's tile.
    /// </summary>
    [Serializable]
    public class LevelTileData
    {
        public TileType TileType;
        public ColorTileType ColorTileType;
        public RandomColorTileType RandomColorTileType;
        public BlockerType BlockerType;
        public CollectibleType CollectibleType;
        public SlotType SlotType;
    }
}
