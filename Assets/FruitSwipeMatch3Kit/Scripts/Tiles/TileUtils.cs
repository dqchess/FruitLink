// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store EULA,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
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
            int height)
        {
            var blockersToDestroy = new List<int>(8);
            var entityMgr = World.Active.EntityManager;
            
            foreach (var idx in indices)
            {
                var neighbours = GetNeighbours(idx, tiles, width, height);
                foreach (var neighbour in neighbours)
                {
                    if (entityMgr.HasComponent<BlockerData>(tiles[neighbour]))
                    {
                        var blockerData = entityMgr.GetComponentData<BlockerData>(tiles[neighbour]);
                        if (blockerData.Type == BlockerType.Stone || blockerData.Type == BlockerType.Wood &&
                            !blockersToDestroy.Contains(neighbour))
                            blockersToDestroy.Add(neighbour);
                    }
                }

                if (entityMgr.HasComponent<TileData>(tiles[idx]) ||
                    entityMgr.HasComponent<CollectibleData>(tiles[idx]))
                {
                    DestroyTile(idx, tiles, gos, slots, particlePools);
                }
            }
            
            foreach (var blocker in blockersToDestroy)
                DestroyBlocker(blocker, tiles, gos, particlePools);
            
            SoundPlayer.PlaySoundFx("TileMatch");
        }

        public static List<int> GetNeighbours(
            int idx,
            NativeArray<Entity> tiles,
            int width,
            int height,
            bool includeCross = false)
        {
            var neighbours = new List<int>(includeCross ? 8 : 4);
            
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

            if (!includeCross) return neighbours;
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
            
            if (slots[idx] != null && slots[idx].GetComponent<Slot>() != null)
            {
                var slot = slots[idx].GetComponent<Slot>();
                var slotType = slot.Type;
                
                slots[idx].GetComponent<PooledObject>().Pool.ReturnObject(slots[idx]);
                slots[idx] = null;
                var evt = entityMgr.CreateEntity(typeof(SlotDestroyedEvent));
                entityMgr.SetComponentData(evt, new SlotDestroyedEvent
                {
                    Type = slotType
                });

                var particles = particlePools.GetSlotParticles(slotType);
                particles.transform.position = tile.transform.position;

                PlaySlotSoundFx(slotType);
            }
        }

        public static void DestroySlot(
            GameObject slotGo,
            List<GameObject> slots,
            List<float3> positions,
            ParticlePools particlePools)
        {
            var idx = slots.IndexOf(slotGo);
            if (slots[idx] != null && slots[idx].GetComponent<Slot>() != null)
            {
                var slot = slots[idx].GetComponent<Slot>();
                var slotType = slot.Type;
                
                slots[idx].GetComponent<PooledObject>().Pool.ReturnObject(slots[idx]);
                slots[idx] = null;

                var entityMgr = World.Active.EntityManager;
                var evt = entityMgr.CreateEntity(typeof(SlotDestroyedEvent));
                entityMgr.SetComponentData(evt, new SlotDestroyedEvent
                {
                    Type = slotType
                });

                var particles = particlePools.GetSlotParticles(slotType);
                particles.transform.position = positions[idx];

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
            
            var particles = particlePools.GetBlockerParticles(type);
            particles.transform.position = tile.transform.position;
            
            tile.GetComponent<PooledObject>().Pool.ReturnObject(tile.gameObject);

            tiles[idx] = Entity.Null;
            gos[idx] = null;

            PlayBlockerSoundFx(type);
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
            Assert.IsNotNull(goe);
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
        }
    }
}
