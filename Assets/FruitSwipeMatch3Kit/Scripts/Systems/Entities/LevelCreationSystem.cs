﻿// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store EULA,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Object = UnityEngine.Object;

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
        }

        protected override void OnUpdate()
        {
            mainCamera = Camera.main;

            Entities.ForEach((Entity entity, ref CreateLevelEvent evt) =>
            {
                PostUpdateCommands.DestroyEntity(entity);

                var levelNumber = evt.Number;
                levelData = Resources.Load<LevelData>($"Levels/{levelNumber}");

                var updateMovesSystem = World.GetExistingSystem<UpdateRemainingMovesUiSystem>();
                updateMovesSystem?.Initialize(levelData.Moves);
            });

            CreateLevel();
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
                case TileType.Color:
                    return tilePools.GetColorTile(tileData.ColorTileType);

                case TileType.RandomColor:
                    return tilePools.GetRandomColorTile(tileData.RandomColorTileType);

                case TileType.Blocker:
                    return tilePools.GetBlocker(tileData.BlockerType);

                case TileType.Collectible:
                    return tilePools.GetCollectible(tileData.CollectibleType);
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
            bool firstPoint = false;
            // xử lý cột bên trái
            for (int y = 0; y < Height; ++y)
            {
                for (int x = 0; x < Width; x++)
                {
                    var idx = x + (y * Width);
                    // bỏ qua hole để tìm tile đầu tiên
                    if (levelData.Tiles[idx].TileType == TileType.Hole)
                        continue;
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

            firstPoint = false;
            // xử lý hàng bên dưới
            for (int x = 0; x < Width; x++)
            {
                for (int y = Height - 1; y >= 0; y--)
                {
                    var idx = x + (y * Width);
                    // bỏ qua hole để tìm tile đầu tiên
                    if (levelData.Tiles[idx].TileType == TileType.Hole)
                        continue;
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

            firstPoint = false;
            // xử lý cột bên phải
            for (int y = Height - 1; y >= 0; y--)
            {
                for (int x = Width - 1; x >= 0; x--)
                {
                    var idx = x + (y * Width);
                    // bỏ qua hole để tìm tile đầu tiên
                    if (levelData.Tiles[idx].TileType == TileType.Hole)
                        continue;
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

            firstPoint = false;
            // xử lý hàng bên trên
            for (int x = Width - 1; x >= 0; x--)
            {
                for (int y = 0; y < Height; y++)
                {
                    var idx = x + (y * Width);
                    // bỏ qua hole để tìm tile đầu tiên
                    if (levelData.Tiles[idx].TileType == TileType.Hole)
                        continue;
                    // add point top right
                    AddPoint(terrain, pointHash, x, y, TilePointDirection.TopRight);
                    // add point top left
                    AddPoint(terrain, pointHash, x, y, TilePointDirection.TopLeft);
                    // đã xử lý
                    break;
                }
            }

            terrain.Build();
        }

        private void AddPoint(Ferr2DT_PathTerrain terrain, HashSet<int> hash, int x, int y,
            TilePointDirection direction)
        {
            int idx = x + (y * Width);
            int jdx;
            switch (direction)
            {
                case TilePointDirection.TopLeft:
                    jdx = x + y * (Width + 1);
                    if(hash.Contains(jdx)) return;
                    hash.Add(jdx);
                    terrain.AddPoint(new Vector2(TilePositions[idx].x - spriteHalfWidth,
                                         TilePositions[idx].y + spriteHalfHeight) * 10);
                    break;
                case TilePointDirection.TopRight:
                    jdx = (x + 1) + y * (Width + 1);
                    if(hash.Contains(jdx)) return;
                    hash.Add(jdx);
                    terrain.AddPoint(new Vector2(TilePositions[idx].x + spriteHalfWidth,
                                         TilePositions[idx].y + spriteHalfHeight) * 10);
                    break;
                case TilePointDirection.BotLeft:
                    jdx = x + (y + 1) * (Width + 1);
                    if(hash.Contains(jdx)) return;
                    hash.Add(jdx);
                    terrain.AddPoint(new Vector2(TilePositions[idx].x - spriteHalfWidth,
                                         TilePositions[idx].y - spriteHalfHeight) * 10);
                    break;
                case TilePointDirection.BotRight:
                    jdx = (x + 1) + (y + 1) * (Width + 1);
                    if(hash.Contains(jdx)) return;
                    hash.Add(jdx);
                    terrain.AddPoint(new Vector2(TilePositions[idx].x + spriteHalfWidth,
                                         TilePositions[idx].y - spriteHalfHeight) * 10);
                    break;
            }
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