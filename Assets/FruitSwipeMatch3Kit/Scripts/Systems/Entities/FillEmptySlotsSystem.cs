// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store EULA,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using Unity.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace FruitSwipeMatch3Kit
{
    /// <summary>
    /// This system is responsible for filling the holes left on
    /// the level with new tiles after a match.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(GravitySystem))]
    public class FillEmptySlotsSystem : ComponentSystem
    {
        private EntityQuery query;
        
        private TilePools tilePools;
        private EntityArchetype applyGravityArchetype;
        protected override void OnCreate()
        {
            Enabled = false;
            query = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[]
                {
                    ComponentType.ReadOnly<FillEmptySlotsData>(), 
                },
            });
            applyGravityArchetype = EntityManager.CreateArchetype(typeof(ApplyGravityData));
        }

        public void Initialize()
        {
            tilePools = Object.FindObjectOfType<TilePools>();
        }
        
        protected override void OnUpdate()
        {
            var dataCopy = new FillEmptySlotsData();
          //  var pChunks  = query.CreateArchetypeChunkArray(Allocator.TempJob);
           /*var pendingType= GetArchetypeChunkComponentType<PendingFillSlot>(true);
           var fillDataType = GetArchetypeChunkComponentType<FillEmptySlotsData>(true);
            foreach (var pChunk in pChunks)
            {
                 var pHasPending =  pChunk.Has(pendingType);
            }*/
            Entities.With(query).ForEach((Entity entity, ref FillEmptySlotsData data) =>
            {
                PostUpdateCommands.DestroyEntity(entity);
                dataCopy = data;
            });
           bool dirty = CreateNewTilesInEmptySlots(dataCopy);
           if (dirty)
           {
               PostUpdateCommands.CreateEntity(applyGravityArchetype);
           }
        }

        private bool CreateNewTilesInEmptySlots(FillEmptySlotsData data)
        {
            var levelCreationSystem = World.GetExistingSystem<LevelCreationSystem>();
            var width = levelCreationSystem.Width;
            var height = levelCreationSystem.Height;
            
            var tiles = levelCreationSystem.TileEntities;
            var gos = levelCreationSystem.TileGos;
            bool dirty = false;
            for (var i = 0; i < width; i++)
            {
                var numEmpties = 0;
                for (var j = 0; j < height; j++)
                {
                    var idx = i + (j * width);
                    var tile = tiles[idx];
                    if (tile == Entity.Null && !EntityManager.HasComponent<HoleSlotData>(tile))
                        numEmpties += 1;
                    else if (tile != Entity.Null && EntityManager.HasComponent<BlockerData>(tile))
                        break;
                }
                
                if (numEmpties == 0)
                    continue;

                for (var j = 0; j < height; j++)
                {
                    var idx = i + (j * width);

                    if (tiles[idx] != Entity.Null && EntityManager.HasComponent<BlockerData>(tiles[idx]))
                        break;

                    if (tiles[idx] == Entity.Null &&
                        !EntityManager.HasComponent<HoleSlotData>(tiles[idx]))
                    {
                        dirty = true;
                        var tile = tilePools.GetRandomColorTile();
                        var entity = tile.GetComponent<GameObjectEntity>().Entity;
                        var tilePos = EntityManager.GetComponentData<TilePosition>(entity);
                        tilePos.X = i;
                        tilePos.Y = j;
                        EntityManager.SetComponentData(entity, tilePos);

                        levelCreationSystem.TileEntities[idx] = entity;
                        levelCreationSystem.TileGos[idx] = tile;

                        var tileHeight = tile.GetComponent<SpriteRenderer>().bounds.size.y;
                        var endPos = levelCreationSystem.TilePositions[idx];
                        var startPos = endPos + new float3(0, numEmpties * tileHeight, 0);
                        EntityManager.AddComponentData(entity, new Translation
                        {
                            Value = new float3(endPos.x, endPos.y, 0)
                        });
                        EntityManager.AddComponentData(entity, new LocalToWorld());
                        
                        EntityManager.AddComponentData(entity, new GravityTag());

                        tile.transform.position = startPos;

                        if (idx == data.MatchIndex)
                        {
                            EntityManager.AddComponentData(entity, new AddBoosterData
                            {
                                MatchSize = data.MatchSize,
                                MatchDirection = data.MatchDirection
                            });
                        }
                    }

                }
            }

            return dirty;
        }
    }
}
