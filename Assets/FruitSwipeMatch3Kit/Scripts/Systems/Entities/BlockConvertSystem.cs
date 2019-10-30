using System.Collections;
using System.Collections.Generic;
using FruitSwipeMatch3Kit;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class BlockConvertSystem : ComponentSystem
{
    public EntityQuery query;
    protected override void OnCreate()
    {
        base.OnCreate();
        query = GetEntityQuery(typeof(BlockSlotData), ComponentType.ReadOnly<Slot>());
    }

    protected override void OnUpdate()
    {
        var blocks = query.ToComponentDataArray<BlockSlotData>(Allocator.TempJob);
        var levelCreationSystem = World.GetExistingSystem<LevelCreationSystem>();
        var tileEntities = levelCreationSystem.TileEntities;
        foreach (var block in blocks)
        {
            if (!EntityManager.HasComponent<BlockerData>(tileEntities[block.tilePosition]))
            {
                EntityManager.AddComponentData(tileEntities[block.tilePosition],new BlockerData()
                {
            
                });
            }
        }
        blocks.Dispose();
    }
}
