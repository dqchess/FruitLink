// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store EULA,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

namespace FruitSwipeMatch3Kit
{
    /// <summary>
    /// This system is responsible for making the tiles that become floating
    /// after a match fall down.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public class GravitySystem : JobComponentSystem
    {
        private EntityQuery query;
        private EntityArchetype fillEmptySlotsArchetype;
        private EndSimulationEntityCommandBufferSystem barrier;

        protected override void OnCreate()
        {
            Enabled = false;
            query = GetEntityQuery(
                ComponentType.ReadOnly<ApplyGravityData>());
            fillEmptySlotsArchetype = EntityManager.CreateArchetype(typeof(FillEmptySlotsData));
            barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var data = query.ToComponentDataArray<ApplyGravityData>(Allocator.TempJob);
            var matchSize = data[0].MatchSize;
            var matchIndex = data[0].MatchIndex;
            var matchDir = data[0].MatchDirection;
            data.Dispose();
            
            return ApplyGravity(inputDeps, matchSize, matchIndex, matchDir);
        }

        private JobHandle ApplyGravity(
            JobHandle inputDeps,
            int matchSize,
            int matchIndex,
            MoveDirection matchDir)
        {
            var levelCreationSystem = World.GetExistingSystem<LevelCreationSystem>();
            var tileEntities = levelCreationSystem.TileEntities;
            var width = levelCreationSystem.Width;
            var height = levelCreationSystem.Height;
            var spriteWidth = levelCreationSystem.GetSpriteWidth();
            var spriteHeight = levelCreationSystem.GetSpriteHeight();

            var job = new ApplyGravityJob
            {
                Ecb = barrier.CreateCommandBuffer(),
                Tiles = tileEntities,
                TilePosition = GetComponentDataFromEntity<TilePosition>(),
                TranslationData = GetComponentDataFromEntity<Translation>(),
                Width = width,
                Height = height,
                HoleSlotData = GetComponentDataFromEntity<HoleSlotData>(true),
                BlockerData = GetComponentDataFromEntity<BlockerData>(true),
                SpriteWidth = spriteWidth,
                SpriteHeight = spriteHeight
            };

            inputDeps = job.Schedule(inputDeps);
            barrier.AddJobHandleForProducer(inputDeps);
            inputDeps.Complete();

//            var inputSystem = World.GetExistingSystem<PlayerInputSystem>();
//            if (!inputSystem.IsBoosterExploding())
//            {
            var boosterTile = tileEntities[matchIndex];
            if (EntityManager.Exists(boosterTile) && !EntityManager.HasComponent<AddBoosterData>(boosterTile))
            {
                EntityManager.AddComponentData(boosterTile, new AddBoosterData
                {
                    MatchSize = matchSize,
                    MatchDirection = matchDir
                });
            }
//            }

            var e = EntityManager.CreateEntity(fillEmptySlotsArchetype);
            EntityManager.SetComponentData(e, new FillEmptySlotsData
            {
                MatchIndex = matchIndex,
                MatchSize = matchSize,
                MatchDirection = matchDir
            });

            var entities = query.ToEntityArray(Allocator.TempJob);
            for (var i = 0; i < entities.Length; ++i)
                EntityManager.DestroyEntity(entities[i]);
            entities.Dispose();
            
            return inputDeps;
        }
    }
}
