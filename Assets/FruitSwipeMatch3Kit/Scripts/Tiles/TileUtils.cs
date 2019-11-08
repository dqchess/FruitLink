// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store EULA,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Assertions;

namespace FruitSwipeMatch3Kit
{
    public static class TileUtils
    {
        public static void DestroyTiles(
            List<int> indices,
            NativeArray<Entity> tiles,
            List<GameObject> gos,
            List<GameObject> slots,
            ParticlePools particlePools,
            int width,
            int height,
            bool isBooster = false)
        {
            var blockersToDestroy = new List<int>(8);
            var entityMgr = World.Active.EntityManager;
            
            foreach (var idx in indices)
            {
                if (!isBooster)
                {
                    var neighbours = GetNeighbours(idx, tiles, width, height);
                    foreach (var neighbour in neighbours)
                    {
                        AddBlockersToDestroy(neighbour, entityMgr, tiles[neighbour], blockersToDestroy);
                    }   
                }
                else
                {
                    AddBlockersToDestroy(idx, entityMgr, tiles[idx], blockersToDestroy);
                }

                if (entityMgr.HasComponent<TileData>(tiles[idx]) || entityMgr.HasComponent<CollectibleData>(tiles[idx]))
                {
                    DestroyTile(idx, tiles, gos, slots, particlePools);
                }
            }
            
            foreach (var blocker in blockersToDestroy)
                DestroyBlocker(blocker, tiles, gos, particlePools);
            
            SoundPlayer.PlaySoundFx("TileMatch");
        }

        private static void AddBlockersToDestroy(int idx, EntityManager entityMgr, Entity tile, List<int> blockersToDestroy)
        {
            if (entityMgr.HasComponent<BlockerData>(tile))
            {
                var blockerData = entityMgr.GetComponentData<BlockerData>(tile);
                if (blockerData.Type == BlockerType.Stone || blockerData.Type == BlockerType.Wood ||
                    blockerData.Type == BlockerType.Stone2 || blockerData.Type == BlockerType.Wood2 ||
                    blockerData.Type == BlockerType.Stone3 || blockerData.Type == BlockerType.Wood3)
                    if(!blockersToDestroy.Contains(idx)) blockersToDestroy.Add(idx);
            }
        }

        public static List<int> GetNeighbours(
            int idx,
            NativeArray<Entity> tiles,
            int width,
            int height,
            bool includeDiagonal = false)
        {
            var neighbours = new List<int>(includeDiagonal ? 8 : 4);
            
            var x = idx % width;
            var y = idx / width;

            var topY = y - 1;
            var top = x + topY * width;

            var bottomY = y + 1;
            var bottom = x + bottomY * width;

            var leftX = x - 1;
            var left = leftX + y * width;

            var rightX = x + 1;
            var right = rightX + y * width;

            if (topY >= 0)
                AddNeighbour(top, tiles, neighbours);

            if (bottomY < height)
                AddNeighbour(bottom, tiles, neighbours);

            if (leftX >= 0)
                AddNeighbour(left, tiles, neighbours);

            if (rightX < width)
                AddNeighbour(right, tiles, neighbours);

            if (!includeDiagonal) return neighbours;
            // top left
            if (topY >= 0 && leftX >= 0)
                AddNeighbour(leftX + topY * width, tiles, neighbours);
            // top right
            if(topY >= 0 && rightX < width)
                AddNeighbour(rightX + topY * width, tiles, neighbours);
            // bot left
            if(leftX >= 0 && bottomY < height)
                AddNeighbour(leftX + bottomY * width, tiles, neighbours);
            // bot right
            if(rightX < width && bottomY < height)
                AddNeighbour(rightX + bottomY * width, tiles, neighbours);
            
            return neighbours;
        }

        public static void DestroyTile(
            int idx,
            NativeArray<Entity> tiles,
            List<GameObject> gos,
            List<GameObject> slots,
            ParticlePools particlePools,
            bool destroyCollectibles = false)
        {
            var tileEntity = tiles[idx];
            
            var tile = gos[idx];
            if (tile == null)
                return;
           
            var world = World.Active;
            var entityMgr = world.EntityManager;
            if (entityMgr.HasComponent<TileData>(tileEntity))
            {
                var evt = entityMgr.CreateEntity(typeof(TileDestroyedEvent));
                var type = entityMgr.GetComponentData<TileData>(tileEntity).Type;
                entityMgr.SetComponentData(evt, new TileDestroyedEvent
                {
                    Type = type
                });
                
                var particles = particlePools.GetColorTileParticles(type);
                particles.transform.position = tile.transform.position;

                tile.GetComponent<PooledObject>().Pool.ReturnObject(tile.gameObject);
                tiles[idx] = Entity.Null;
                gos[idx] = null;
            }
            else if (entityMgr.HasComponent<CollectibleData>(tileEntity) && destroyCollectibles)
            {
                var evt = entityMgr.CreateEntity(typeof(CollectibleCollectedEvent));
                var type = entityMgr.GetComponentData<CollectibleData>(tileEntity).Type;
                entityMgr.SetComponentData(evt, new CollectibleCollectedEvent
                {
                    Type = type
                });
                
                var particles = particlePools.GetCollectibleParticles(type);
                particles.transform.position = tile.transform.position;
                
                tile.GetComponent<PooledObject>().Pool.ReturnObject(tile.gameObject);
                tiles[idx] = Entity.Null;
                gos[idx] = null;
            }
            DestroySlot(idx, tile, slots, particlePools);
        }

        public static void DestroySlot(int idx,
            GameObject slotGo,
            List<GameObject> slots,
            ParticlePools particlePools)
        {
            if (slots[idx] != null && slots[idx].GetComponent<Slot>() != null)
            {
                var slot = slots[idx].GetComponent<Slot>();
                var slotType = slot.Type;
                var tilePool = World.Active.GetExistingSystem<LevelCreationSystem>().TilePools;
                Vector3 pos = slots[idx].transform.position;
                switch (slotType)
                {
                    case SlotType.Ice3:
                        slots[idx].GetComponent<PooledObject>().Pool.ReturnObject(slots[idx]);
                        slots[idx] = tilePool.GetSlot(SlotType.Ice2);
                        slots[idx].transform.position = pos;
                        break;
                    case SlotType.Ice2:
                        slots[idx].GetComponent<PooledObject>().Pool.ReturnObject(slots[idx]);
                        slots[idx] = tilePool.GetSlot(SlotType.Ice);
                        slots[idx].transform.position = pos;
                        break;
                    case SlotType.Jelly3:
                        slots[idx].GetComponent<PooledObject>().Pool.ReturnObject(slots[idx]);
                        slots[idx] = tilePool.GetSlot(SlotType.Jelly2);
                        slots[idx].transform.position = pos;
                        break;
                    case SlotType.Jelly2:
                        slots[idx].GetComponent<PooledObject>().Pool.ReturnObject(slots[idx]);
                        slots[idx] = tilePool.GetSlot(SlotType.Jelly);
                        slots[idx].transform.position = pos;
                        break;
                    default:
                        slots[idx].GetComponent<PooledObject>().Pool.ReturnObject(slots[idx]);
                        slots[idx] = null;       
                        break;
                }
                var entityMgr = World.Active.EntityManager;
                var evt = entityMgr.CreateEntity(typeof(SlotDestroyedEvent));
                entityMgr.SetComponentData(evt, new SlotDestroyedEvent
                {
                    Type = slotType
                });

                var particles = particlePools.GetSlotParticles(slotType);
                particles.transform.position = slotGo.transform.position;

                PlaySlotSoundFx(slotType);
            }
        }
        
        public static void DestroyBlocker(
            int idx,
            NativeArray<Entity> tiles,
            List<GameObject> gos,
            ParticlePools particlePools)
        {
            var tileEntity = tiles[idx];
            
            var tile = gos[idx];
            if (tile == null)
                return;

            var world = World.Active;
            var entityMgr = world.EntityManager;
            var evt = entityMgr.CreateEntity(typeof(BlockerDestroyedEvent));
            var type = entityMgr.GetComponentData<BlockerData>(tileEntity).Type;
            entityMgr.SetComponentData(evt, new BlockerDestroyedEvent
            {
                Type = type
            });
            var tilePool = world.GetExistingSystem<LevelCreationSystem>().TilePools;
            var particles = particlePools.GetBlockerParticles(type);
            particles.transform.position = tile.transform.position;
            if (type == BlockerType.Stone3)
            {
                gos[idx] = tilePool.GetBlocker(BlockerType.Stone2);
                tiles[idx] = ReplaceTile(entityMgr, tileEntity, tile, gos[idx]);
            }
            else if (type == BlockerType.Stone2)
            {
                gos[idx] = tilePool.GetBlocker(BlockerType.Stone);
                tiles[idx] = ReplaceTile(entityMgr, tileEntity, tile, gos[idx]);
            }
            else if (type == BlockerType.Wood3)
            {
                gos[idx] = tilePool.GetBlocker(BlockerType.Wood2);
                tiles[idx] = ReplaceTile(entityMgr, tileEntity, tile, gos[idx]);
            }
            else if (type == BlockerType.Wood2)
            {
                gos[idx] = tilePool.GetBlocker(BlockerType.Wood);
                tiles[idx] = ReplaceTile(entityMgr, tileEntity, tile, gos[idx]);
            }
            else
            {
                tile.GetComponent<PooledObject>().Pool.ReturnObject(tile.gameObject);
                tiles[idx] = Entity.Null;
                gos[idx] = null;   
            }

            PlayBlockerSoundFx(type);
        }

        private static Entity ReplaceTile(EntityManager entityMgr, Entity tileEntity, GameObject tile, GameObject go)
        {
            var entity = go.GetComponent<GameObjectEntity>().Entity;
            var tilePos = entityMgr.GetComponentData<TilePosition>(tileEntity);
            var translation = entityMgr.GetComponentData<Translation>(tileEntity);
            entityMgr.SetComponentData(entity, tilePos);
            entityMgr.AddComponentData(entity, translation);
            entityMgr.AddComponentData(entity, new LocalToWorld());
            go.transform.position = tile.transform.position;
            tile.GetComponent<PooledObject>().Pool.ReturnObject(tile.gameObject);
            return entity;
        }

        private static void AddNeighbour(
            int idx,
            NativeArray<Entity> tiles,
            List<int> neighbours)
        {
            var tile = tiles[idx];
            if (tile != Entity.Null)
                neighbours.Add(idx);
        }

        private static void PlaySlotSoundFx(SlotType type)
        {
            switch (type)
            {
                case SlotType.Ice:
                    SoundPlayer.PlaySoundFx("Ice");
                    break;
                
                case SlotType.Jelly:
                    SoundPlayer.PlaySoundFx("Jelly");
                    break;
            }
        }
        
        private static void PlayBlockerSoundFx(BlockerType type)
        {
            switch (type)
            {
                case BlockerType.Stone:
                    SoundPlayer.PlaySoundFx("Stone");
                    break;
                
                case BlockerType.Wood:
                    SoundPlayer.PlaySoundFx("Wood");
                    break;
            }
        }
        
        public static void RemoveBoosterToTile(GameObject go, EntityManager entityManager)
        {
            var goe = go.GetComponent<GameObjectEntity>();
            Assert.IsNotNull(goe);
            var entity = goe.Entity;
            if (!entityManager.HasComponent<BoosterData>(entity))
            {
                var tile = go.GetComponent<Tile>();
                if (tile != null)
                {
                    entityManager.RemoveComponent<BoosterData>(entity);
                    tile.RemoveBooster();
                }
            }
        }

        public static void AddBoosterToTile(GameObject go, BoosterType type, EntityManager entityManager)
        {
            var goe = go.GetComponent<GameObjectEntity>();
            Assert.IsNotNull(goe);
            var entity = goe.Entity;
            if (!entityManager.HasComponent<BoosterData>(entity))
            {
                var tile = go.GetComponent<Tile>();
                if (tile != null)
                {
                    entityManager.AddComponentData(entity, new BoosterData
                    {
                        Type = type
                    });

                    tile.AddBooster(type);
                }
            }
        }
        
        public static void AddBoosterToTile(
            GameObject go, BoosterType type, EntityManager entityManager, EntityCommandBuffer puc)
        {
            var goe = go.GetComponent<GameObjectEntity>();
            var entity = goe.Entity;
            if (!entityManager.HasComponent<BoosterData>(entity))
            {
                var tile = go.GetComponent<Tile>();
                if (tile != null)
                {
                    puc.AddComponent(entity, new BoosterData
                    {
                        Type = type
                    });

                    tile.AddBooster(type);
                }
            }
            else
            {
                BoosterType old = entityManager.GetComponentData<BoosterData>(entity).Type;
                int oldPoint = BoosterPoint(old);
                int newPoint = BoosterPoint(type);
                if (oldPoint > newPoint) return;
                var tile = go.GetComponent<Tile>();
                if (tile != null)
                {
                    puc.SetComponent(entity, new BoosterData
                    {
                        Type = type
                    });

                    tile.ReplaceBooster(type);
                }
            }
        }

        private static int BoosterPoint(BoosterType type)
        {
            if (type == BoosterType.Horizontal || type == BoosterType.Vertical || 
                type == BoosterType.DiagonalLeft || type == BoosterType.DiagonalRight)
                return 1;
            if (type == BoosterType.Cross || type == BoosterType.X)
                return 2;
            if (type == BoosterType.Star)
                return 3;
            return 1;
        }
    }
}
