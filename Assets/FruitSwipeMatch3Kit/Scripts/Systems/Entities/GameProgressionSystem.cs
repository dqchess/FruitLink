// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store EULA,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using Unity.Entities;

namespace FruitSwipeMatch3Kit
{
    /// <summary>
    /// This system keeps track of whether the player is on the game
    /// screen after transitioning from the level screen or not. This
    /// is useful to know because the game can be directly played from
    /// the game screen (for testing purposes).
    /// </summary>
    public class GameProgressionSystem : ComponentSystem
    {
        public bool IsPlayerComingFromLevelScreen;

        private EntityQuery query;
        
        protected override void OnUpdate()
        {
        }
    }
}
