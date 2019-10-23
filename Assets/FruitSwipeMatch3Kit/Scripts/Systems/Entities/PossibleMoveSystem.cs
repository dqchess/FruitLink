using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

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
            var width = levelCreationSystem.Width;
            var height = levelCreationSystem.Height;

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
    }
}