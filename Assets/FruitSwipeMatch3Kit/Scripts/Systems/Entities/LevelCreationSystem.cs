// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store EULA,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;
using DG.Tweening;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace FruitSwipeMatch3Kit
{
    /// <summary>
    /// This system is responsible for creating the runtime game levels.
    /// It processes entities with the LevelCreationData component attached
    /// to them, which in turn contain the number of the level to create.
    /// 
    /// The configuration of the specific level to create is loaded dynamically
    /// from the Resources folder, where it is stored as a Unity asset.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public class LevelCreationSystem : ComponentSystem, IRestartable
    {
        public int Width;
        public int Height;
        public NativeArray<Entity> TileEntities;
        public List<GameObject> TileGos;
        public List<GameObject> Slots;
        public List<float3> TilePositions;

        private Camera mainCamera;

        private float spriteWidth;
        private float spriteHeight;
        private float spriteHalfWidth;
        private float spriteHalfHeight;
        private float totalWidth;
        private float totalHeight;
        private const float HorizontalSpacing = 0.0f;
        private const float VerticalSpacing = 0.0f;

        private LevelData levelData;
        private TilePools tilePools;
        public TilePools TilePools => tilePools;

        protected override void OnCreate()
        {
            Enabled = false;
        }

        protected override void OnDestroy()
        {
            if (TileEntities.IsCreated)
                TileEntities.Dispose();
        }

        public void Initialize()
        {
            tilePools = Object.FindObjectOfType<TilePools>();
            var temp = tilePools.GetColorTile(ColorTileType.Color1);
            var bounds = temp.GetComponent<SpriteRenderer>().sprite.bounds;
            spriteWidth = bounds.size.x;
            spriteHeight = bounds.size.y;
            spriteHalfWidth = spriteWidth / 2;
            spriteHalfHeight = spriteHeight / 2;
            temp.GetComponent<PooledObject>().Pool.ReturnObject(temp);
            mainCamera = Camera.main;
        }

        protected override void OnUpdate()
        {
            Entities.ForEach((Entity entity, ref CreateLevelEvent evt) =>
            {
                PostUpdateCommands.DestroyEntity(entity);

                var levelNumber = evt.Number;
                levelData = Resources.Load<LevelData>($"Levels/{levelNumber}");

                var updateMovesSystem = World.GetExistingSystem<UpdateRemainingMovesUiSystem>();
                updateMovesSystem?.Initialize(levelData.Moves);
            });
            
            CreateLevel();
            var seg = DOTween.Sequence();
            seg.AppendInterval(GameplayConstants.OpenPopupDelay);
            seg.AppendCallback(() =>
            {
                EntityManager.CreateEntity(ComponentType.ReadOnly<MatchEndEvent>());
                EntityManager.CreateEntity(ComponentType.ReadOnly<JellyDestroyedEvent>());
            });
        }

        private void CreateLevel()
        {
            Width = levelData.Width;
            Height = levelData.Height;
            var levelSize = Width * Height;

            if (TileEntities.IsCreated)
                TileEntities.Dispose();

            TileEntities = new NativeArray<Entity>(levelSize, Allocator.Persistent);
            TileGos = new List<GameObject>(levelSize);
            Slots = new List<GameObject>(levelSize);

            TilePositions = new List<float3>(levelSize);

            CreateTiles();
            CenterTilesOnScreen();
            CreateBackground();
            CreateMainBorderTerrain();
            CreateSlots();
            ZoomMainCamera();
        }

        private void CreateTiles()
        {
            for (var j = 0; j < Height; ++j)
            {
                for (var i = 0; i < Width; ++i)
                {
                    var idx = i + (j * Width);
                    var tileGo = CreateTile(levelData.Tiles[idx]);

                    TileGos.Add(tileGo);

                    var pos = new float3(
                        i * (spriteWidth + HorizontalSpacing),
                        -j * (spriteHeight + VerticalSpacing),
                        0);
                    TilePositions.Add(pos);

                    if (tileGo == null)
                    {
                        if (levelData.Tiles[idx].TileType == TileType.Hole)
                        {
                            var archetype = EntityManager.CreateArchetype(typeof(HoleSlotData));
                            var e = EntityManager.CreateEntity(archetype);
                            TileEntities[idx] = e;
                            continue;
                        }
                        
                        if (levelData.Tiles[idx].TileType == TileType.Empty)
                        {
                            TileEntities[idx] = Entity.Null;
                            continue;
                        }
                    }

                    var tileEntity = tileGo.GetComponent<GameObjectEntity>().Entity;
                    var tilePos = EntityManager.GetComponentData<TilePosition>(tileEntity);
                    tilePos.X = i;
                    tilePos.Y = j;
                    EntityManager.SetComponentData(tileEntity, tilePos);
                    if (levelData.Tiles[idx].TileType == TileType.HoleImage)
                        EntityManager.AddComponentData(tileEntity, new HoleSlotData());
                    TileEntities[idx] = tileEntity;

                    EntityManager.AddComponentData(tileEntity, new Translation
                    {
                        Value = new float3
                        {
                            x = i * (spriteWidth + HorizontalSpacing),
                            y = -j * (spriteHeight + VerticalSpacing),
                            z = 0
                        }
                    });
                    
                    EntityManager.AddComponentData(tileEntity, new LocalToWorld());
                }
            }

            totalWidth = (Width - 1) * (spriteWidth + HorizontalSpacing);
            totalHeight = (Height - 1) * (spriteHeight + VerticalSpacing);
        }

        private GameObject CreateTile(LevelTileData tileData)
        {
            switch (tileData.TileType)
            {
                case TileType.Random:
                    return tilePools.GetRandomColorTile();
                
                case TileType.Color:
                    return tilePools.GetColorTile(tileData.ColorTileType);

                case TileType.RandomColor:
                    return tilePools.GetRandomColorTile(tileData.RandomColorTileType);

                case TileType.Blocker:
                    return tilePools.GetBlocker(tileData.BlockerType);

                case TileType.Collectible:
                    return tilePools.GetCollectible(tileData.CollectibleType);
                
                case TileType.HoleImage:
                    return tilePools.GetHoleImage();
            }

            return null;
        }

        private GameObject CreateSlot(LevelTileData tileData)
        {
            if (tileData.SlotType == SlotType.Normal)
                return null;

            return tilePools.GetSlot(tileData.SlotType);
        }

        private void CenterTilesOnScreen()
        {
            int size = levelData.Width > levelData.Height ? levelData.Width : levelData.Height; 
            var levelLocation = Vector3.zero - new Vector3(0.0f, size * 0.26f, 0.0f);

            var i = 0;
            foreach (var tile in TileGos)
            {
                TilePositions[i] += new float3(-totalWidth / 2, totalHeight / 2 + levelLocation.y, 0);
                if (tile != null)
                {
                    tile.transform.position = TilePositions[i];
                    var tileEntity = tile.gameObject.GetComponent<GameObjectEntity>().Entity;
                    EntityManager.SetComponentData(tileEntity, new Translation
                    {
                        Value = TilePositions[i]
                    });
                }

                i += 1;
            }
        }

        private void CreateBackground()
        {
            for (var j = 0; j < Height; ++j)
            {
                for (var i = 0; i < Width; ++i)
                {
                    var idx = i + (j * Width);
                    if (levelData.Tiles[idx].TileType == TileType.Hole)
                        continue;

                    GameObject tile;
                    if (j % 2 == 0)
                        tile = i % 2 == 0
                            ? tilePools.GetDarkBackgroundTile()
                            : tilePools.GetLightBackgroundTile();
                    else
                        tile = i % 2 == 0
                            ? tilePools.GetLightBackgroundTile()
                            : tilePools.GetDarkBackgroundTile();

                    tile.transform.position = TilePositions[idx];
                }
            }
        }
        enum TilePointDirection
        {
            TopLeft,
            TopRight,
            BotLeft,
            BotRight
        }

        private void CreateMainBorderTerrain()
        {
            Ferr2DT_PathTerrain terrain = tilePools.BorderTerrain;
            terrain.ClearPoints();
            HashSet<int> pointHash = new HashSet<int>();
            int lastX = 0, lastY = 0;
            ProcessColumnLeft(terrain, pointHash, ref lastX, ref lastY);

            ProcessRowBottom(terrain, pointHash, ref lastX, ref lastY);

            ProcessColumnRight(terrain, pointHash, ref lastX, ref lastY);
            
            ProcessRowTop(terrain, pointHash, ref lastX, ref lastY);

            terrain.Build();
        }

        private void ProcessColumnLeft(Ferr2DT_PathTerrain terrain, HashSet<int> pointHash, ref int lastX, ref int lastY)
        {
            bool firstPoint = false;
            for (int y = 0; y < Height; ++y)
            {
                for (int x = 0; x < Width; x++)
                {
                    var idx = x + (y * Width);
                    // bỏ qua hole để tìm tile đầu tiên
                    if (x != Width - 1 && levelData.Tiles[idx].TileType == TileType.Hole) continue;
                    if (x == Width - 1 && levelData.Tiles[idx].TileType == TileType.Hole)
                    {
                        terrain.ClearPoints();
                        pointHash.Clear();
                        continue;    
                    }
                    // add point to ignore conflict
                    if (!firstPoint) firstPoint = true;
                    else
                    {
                        if (lastX - x > 1)
                        {
                            for (int i = x; i < lastX - 1; i++)
                            {
                                AddPoint(terrain, pointHash, i, y, TilePointDirection.TopRight);
                            }
                        }
                        if (x - lastX > 1)
                        {
                            for (int i = lastX; i < x - 1; i++)
                            {
                                AddPoint(terrain, pointHash, i, lastY, TilePointDirection.BotRight);
                            }
                        }
                    }
                    // add point top left
                    AddPoint(terrain, pointHash, x, y, TilePointDirection.TopLeft);
                    // add point bot left
                    AddPoint(terrain, pointHash, x, y, TilePointDirection.BotLeft);
                    // đã xử lý
                    lastX = x;
                    lastY = y;
                    break;
                }
            }
        }

        private void ProcessRowBottom(Ferr2DT_PathTerrain terrain, HashSet<int> pointHash, ref int lastX, ref int lastY)
        {
            bool firstPoint = false;
            for (int x = 0; x < Width; x++)
            {
                for (int y = Height - 1; y >= 0; y--)
                {
                    var idx = x + (y * Width);
                    // bỏ qua hole để tìm tile đầu tiên
                    if (y != 0 && levelData.Tiles[idx].TileType == TileType.Hole) continue;
                    if (y == 0 && levelData.Tiles[idx].TileType == TileType.Hole) return;
                    // add point to ignore conflict
                    if (!firstPoint) firstPoint = true;
                    else
                    {
                        if (lastY - y > 1)
                        {
                            for (int i = lastY; i > y + 1; i--)
                            {
                                AddPoint(terrain, pointHash, lastX, i, TilePointDirection.TopRight);
                            }
                        }
                    }
                    // add point bot left
                    AddPoint(terrain, pointHash, x, y, TilePointDirection.BotLeft);
                    // add point bot right
                    AddPoint(terrain, pointHash, x, y, TilePointDirection.BotRight);
                    // đã xử lý
                    lastX = x;
                    lastY = y;
                    break;
                }
            }
        }

        private void ProcessColumnRight(Ferr2DT_PathTerrain terrain, HashSet<int> pointHash, ref int lastX, ref int lastY)
        {
            bool firstPoint = false;
            for (int y = Height - 1; y >= 0; y--)
            {
                for (int x = Width - 1; x >= 0; x--)
                {
                    var idx = x + (y * Width);
                    // bỏ qua hole để tìm tile đầu tiên
                    if (x != 0 && levelData.Tiles[idx].TileType == TileType.Hole) continue;
                    if (x == 0 && levelData.Tiles[idx].TileType == TileType.Hole) return;
                    if (!firstPoint) firstPoint = true;
                    else
                    {
                        if (lastX - x > 1)
                        {
                            for (int i = lastX; i > x + 1; i--)
                            {
                                AddPoint(terrain, pointHash, i, lastY, TilePointDirection.TopLeft);
                            }
                        }
                    }
                    // add point bot right
                    AddPoint(terrain, pointHash, x, y, TilePointDirection.BotRight);
                    // add point top right
                    AddPoint(terrain, pointHash, x, y, TilePointDirection.TopRight);
                    // đã xử lý
                    lastX = x;
                    lastY = y;
                    break;
                }
            }
        }

        private void ProcessRowTop(Ferr2DT_PathTerrain terrain, HashSet<int> pointHash, ref int lastX, ref int lastY)
        {
            List<int> lastPoint = new List<int>();
            for (int x = Width - 1; x >= 0; x--)
            {
                for (int y = 0; y < Height; y++)
                {
                    var idx = x + (y * Width);
                    // bỏ qua hole để tìm tile đầu tiên
                    if (y != Height - 1 && levelData.Tiles[idx].TileType == TileType.Hole) continue;
                    if (y == Height - 1 && levelData.Tiles[idx].TileType == TileType.Hole)
                    {
                        for (int i = lastPoint.Count - 1; i >= 0; i--)
                        {
                            terrain.RemovePoint(lastPoint[i]);
                        }
                        return;
                    }
                    // add point top right
                    int point1Index = AddPoint(terrain, pointHash, x, y, TilePointDirection.TopRight);
                    // add point top left
                    int point2Index = AddPoint(terrain, pointHash, x, y, TilePointDirection.TopLeft);
                    if(point1Index != -1) lastPoint.Add(point1Index);
                    if(point2Index != -1) lastPoint.Add(point2Index);
                    // đã xử lý
                    break;
                }
            }
        }

        private int AddPoint(Ferr2DT_PathTerrain terrain, HashSet<int> hash, int x, int y,
            TilePointDirection direction)
        {
            int idx = x + (y * Width);
            int jdx;
            switch (direction)
            {
                case TilePointDirection.TopLeft:
                    jdx = x + y * (Width + 1);
                    if(hash.Contains(jdx)) return -1;
                    hash.Add(jdx);
                    return terrain.AddPoint(new Vector2(TilePositions[idx].x - spriteHalfWidth,
                                         TilePositions[idx].y + spriteHalfHeight) * 10);
                case TilePointDirection.TopRight:
                    jdx = (x + 1) + y * (Width + 1);
                    if(hash.Contains(jdx)) return -1;
                    hash.Add(jdx);
                    return terrain.AddPoint(new Vector2(TilePositions[idx].x + spriteHalfWidth,
                                         TilePositions[idx].y + spriteHalfHeight) * 10);
                case TilePointDirection.BotLeft:
                    jdx = x + (y + 1) * (Width + 1);
                    if(hash.Contains(jdx)) return -1;
                    hash.Add(jdx);
                    return terrain.AddPoint(new Vector2(TilePositions[idx].x - spriteHalfWidth,
                                         TilePositions[idx].y - spriteHalfHeight) * 10);
                case TilePointDirection.BotRight:
                    jdx = (x + 1) + (y + 1) * (Width + 1);
                    if(hash.Contains(jdx)) return -1;
                    hash.Add(jdx);
                    return terrain.AddPoint(new Vector2(TilePositions[idx].x + spriteHalfWidth,
                                         TilePositions[idx].y - spriteHalfHeight) * 10);
            }

            return -1;
        }

        private void CreateSlots()
        {
            for (var j = 0; j < Height; ++j)
            {
                for (var i = 0; i < Width; ++i)
                {
                    var idx = i + (j * Width);
                
                    var slot = CreateSlot(levelData.Tiles[idx]);
                    Slots.Add(slot);

                    if (slot != null)
                        slot.transform.position = TilePositions[idx];
                }
            }
        }

        public void SpawnRandomJelly()
        {
            List<int> indexList = new List<int>();
            for (int i = 0; i < Slots.Count; i++)
            {
                if(Slots[i] != null) indexList.Add(i);
            }
            for (int i = 0; i < Slots.Count; i++)
            {
                int idx = Random.Range(0, indexList.Count);
                idx = indexList[idx];
                idx = GetNormalSlotNeighbour(idx);
                indexList.RemoveAt(idx);
                if (idx != -1)
                {
                    var slot = tilePools.GetSlot(SlotType.Jelly);
                    Slots[idx] = slot;
                    slot.transform.position = TilePositions[idx];
                    var evt = EntityManager.CreateEntity(typeof(SlotInstantiatedEvent));
                    EntityManager.SetComponentData(evt, new SlotInstantiatedEvent
                    {
                        Type = SlotType.Jelly
                    });
                    return;
                }
            }
        }

        private int GetNormalSlotNeighbour(int idx)
        {
            List<int> neighbours = TileUtils.GetNeighbours(idx, TileEntities, Width, Height);
            for (int i = 0; i < neighbours.Count; i++)
            {
                if(Slots[neighbours[i]] != null || 
                   EntityManager.HasComponent<HoleSlotData>(TileEntities[neighbours[i]]) || 
                   EntityManager.HasComponent<BlockerData>(TileEntities[neighbours[i]])) continue;
                return neighbours[i];
            }

            return -1;
        }

        private void ZoomMainCamera()
        {
            var gameScreen = Object.FindObjectOfType<GameScreen>();
            var zoomLevel = gameScreen.GameConfig.GetZoomLevel();
            if (levelData.Width < 7)
                zoomLevel *= 1.5f;
            float total = totalHeight > totalWidth ? totalHeight : totalWidth;
            mainCamera.orthographicSize = total * zoomLevel * (Screen.height / (float) Screen.width) * 0.5f;
            float sizeY = Screen.height - Screen.height * 0.4375f;
            Debug.Log("X: " + Screen.width + " " + "Y: " + sizeY);
            if (Screen.width > sizeY)
            {
                if (levelData.Width >= 7)
                    mainCamera.orthographicSize /= 0.755f;
            }
        }

        public float GetSpriteWidth()
        {
            return spriteWidth;
        }
        
        public float GetSpriteHeight()
        {
            return spriteHeight;
        }

        public void OnGameRestarted()
        {
            foreach (var pool in tilePools.GetComponentsInChildren<ObjectPool>())
                pool.Reset();

            CreateLevel();
        }
    }
}