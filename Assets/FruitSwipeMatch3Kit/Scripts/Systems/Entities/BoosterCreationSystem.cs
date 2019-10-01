// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store EULA,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using Unity.Entities;
using UnityEngine;

namespace FruitSwipeMatch3Kit
{
    /// <summary>
    /// This system is responsible for adding boosters to the appropriate tiles.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(FillEmptySlotsSystem))]
    public class BoosterCreationSystem : ComponentSystem
    {
        private EntityQuery query;

        private GameConfiguration gameConfig;
        
        protected override void OnCreate()
        {
            Enabled = false;
            query = GetEntityQuery(
                ComponentType.ReadOnly<Transform>(),
                ComponentType.ReadOnly<AddBoosterData>());
        }

        public void Initialize()
        {
            var gameScreen = Object.FindObjectOfType<GameScreen>();
            gameConfig = gameScreen.GameConfig;
        }

        protected override void OnUpdate()
        {
            Entities.With(query).ForEach((Entity entity, Transform transform, ref AddBoosterData boosterData) =>
            {
                var go = transform.gameObject;
                if (boosterData.MatchSize >= gameConfig.NumTilesNeededForStarBooster)
                {
                    TileUtils.AddBoosterToTile(go, BoosterType.Star, EntityManager, PostUpdateCommands);
                }
                else if (boosterData.MatchSize >= gameConfig.NumTilesNeededForCrossBooster)
                {
                    TileUtils.AddBoosterToTile(go, BoosterType.Cross, EntityManager, PostUpdateCommands);
                }
                else if (boosterData.MatchSize >= gameConfig.NumTilesNeededForRegularBooster)
                {
                    switch (boosterData.MatchDirection)
                    {
                        case MoveDirection.Horizontal:
                            TileUtils.AddBoosterToTile(go, BoosterType.Horizontal, EntityManager, PostUpdateCommands);
                            break;
                        
                        case MoveDirection.Vertical:
                            TileUtils.AddBoosterToTile(go, BoosterType.Vertical, EntityManager, PostUpdateCommands);
                            break;
                        
                        case MoveDirection.DiagonalLeft:
                            TileUtils.AddBoosterToTile(go, BoosterType.DiagonalLeft, EntityManager, PostUpdateCommands);
                            break;
                        
                        case MoveDirection.DiagonalRight:
                            TileUtils.AddBoosterToTile(go, BoosterType.DiagonalRight, EntityManager, PostUpdateCommands);
                            break;
                    }
                }

                PostUpdateCommands.RemoveComponent<AddBoosterData>(entity);
            });
        }
    }
}
