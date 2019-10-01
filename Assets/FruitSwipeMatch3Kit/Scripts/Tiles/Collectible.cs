// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store EULA,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace FruitSwipeMatch3Kit
{
    public class Collectible : MonoBehaviour, IFallingTile
    {
        public void OnTileFell()
        {
            var goEntity = GetComponent<GameObjectEntity>();
            var entityMgr = goEntity.EntityManager;
            var entity = goEntity.Entity;
            var applyGravityArchetype = entityMgr.CreateArchetype(typeof(ApplyGravityData));
            var tilePos = entityMgr.GetComponentData<TilePosition>(entity);
            var levelCreationSystem = World.Active.GetExistingSystem<LevelCreationSystem>();
            var inputSystem = World.Active.GetExistingSystem<PlayerInputSystem>();
            var tiles = levelCreationSystem.TileEntities;
            var width = levelCreationSystem.Width;
            var height = levelCreationSystem.Height;
            if (IsBottomPosition(tilePos, width, height, tiles, entityMgr))
            {
                inputSystem.LockInput();
                
                var particlePools = FindObjectOfType<ParticlePools>();
                var idx = tilePos.X + tilePos.Y * width;
                TileUtils.DestroyTile(
                    idx,
                    levelCreationSystem.TileEntities,
                    levelCreationSystem.TileGos,
                    levelCreationSystem.Slots,
                    particlePools,
                    true);

                if (!inputSystem.IsBoosterExploding())
                {
                    var e = entityMgr.CreateEntity(applyGravityArchetype);
                    entityMgr.SetComponentData(e, new ApplyGravityData
                    {
                        MatchSize = 0,
                        MatchIndex = 0,
                        MatchDirection = MoveDirection.Vertical
                    });
                }

                SoundPlayer.PlaySoundFx("Collectable");
            }
        }

        private bool IsBottomPosition(
            TilePosition tilePos, int width, int height, NativeArray<Entity> tiles, EntityManager entityMgr)
        {
            for (var i = tilePos.Y + 1; i < height; i++)
            {
                var idx = tilePos.X + i * width;
                if (!entityMgr.HasComponent<HoleSlotData>(tiles[idx])) 
                    return false;
            }

            return true;
        }
    }
}
