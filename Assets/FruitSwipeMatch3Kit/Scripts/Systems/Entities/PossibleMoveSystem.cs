using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace FruitSwipeMatch3Kit
{
    [UpdateAfter(typeof(CheckWinConditionSystem))]
    [UpdateBefore(typeof(MatchEndSystem))]
    public class PossibleMoveSystem : JobComponentSystem
    {
        private EntityQuery query;
        private EndSimulationEntityCommandBufferSystem barrier;
        
        protected override void OnCreate()
        {
            Enabled = false;
            GetEntityQuery(ComponentType.ReadOnly<MatchEndEvent>());
            query = GetEntityQuery(ComponentType.ReadOnly<CheckPossibleMoveTag>());
            barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var data = query.ToEntityArray(Allocator.TempJob);
            for (int i = 0; i < data.Length; i++)
            {
                EntityManager.DestroyEntity(data[i]);
            }
            data.Dispose();
            return FindPossibleMove(inputDeps);
        }
        
        private JobHandle FindPossibleMove(JobHandle inputDeps)
        {
            var levelCreationSystem = World.GetExistingSystem<LevelCreationSystem>();
            var tileEntities = levelCreationSystem.TileEntities;
            var slots = levelCreationSystem.Slots;
            var width = levelCreationSystem.Width;
            var height = levelCreationSystem.Height;
            CheckJelly(slots, width, height);
            var job = new PossibleMoveJob()
            {
                Ecb = barrier.CreateCommandBuffer(),
                Tiles = tileEntities,
                Width = width,
                Height = height,
                HoleSlotData = GetComponentDataFromEntity<HoleSlotData>(true),
                BlockerData = GetComponentDataFromEntity<BlockerData>(true),
                TileData = GetComponentDataFromEntity<TileData>(true),
            };

            inputDeps = job.Schedule(inputDeps);
            barrier.AddJobHandleForProducer(inputDeps);
            inputDeps.Complete();
            
            return inputDeps;
        }

        private void CheckJelly(List<GameObject> slots, int width, int height)
        {
            for (var i = 0; i < width; i++)
            {
                for (var j = height - 1; j >= 0; j--)
                {
                    var idx = i + j * width;
                    if (slots[idx] != null)
                    {
                        SlotType slotType = slots[idx].GetComponent<Slot>().Type;
                        if (slotType == SlotType.Jelly || slotType == SlotType.Jelly2 || slotType == SlotType.Jelly3)
                        {
                            GameState.HasJelly = true;
                            return;
                        }
                    }
                }
            }
            GameState.HasJelly = false;
        }
    }
}