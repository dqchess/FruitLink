using Unity.Entities;
using UnityEngine;

namespace FruitSwipeMatch3Kit
{
    [UpdateAfter(typeof(CheckWinConditionSystem))]
    [UpdateAfter(typeof(CollectSlotsGoalTrackingSystem))]
    public class MatchEndSystem : ComponentSystem
    {
        private EntityQuery query;
        
        protected override void OnCreate()
        {
            Enabled = false;
            query = GetEntityQuery(ComponentType.ReadOnly<MatchEndEvent>());
        }

        protected override void OnUpdate()
        {
            bool isMatchEnd = false;
            bool isJellyDestroy = false;
            Entities.With(query).ForEach(entity =>
            {
                isMatchEnd = true;
                Entities.WithAny<JellyDestroyedEvent>().ForEach(entity1 =>
                {
                    isJellyDestroy = true;
                    PostUpdateCommands.DestroyEntity(entity1);
                });
                PostUpdateCommands.DestroyEntity(entity);
            });
            if (isMatchEnd && !isJellyDestroy && GameState.HasJelly)
            {
                World.GetExistingSystem<LevelCreationSystem>().SpawnRandomJelly();
            }
        }
    }
}