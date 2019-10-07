// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store EULA,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;
using System.Collections;
using System.Collections.Generic;
using Ferr;
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
//            CreateBorderTerrain();
            CreateBorderBackground();
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
            var levelLocation = Vector3.zero - new Vector3(0.0f, levelData.Width * 0.13f, 0.0f);

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

        private void CreateBorderTerrain()
        {
            float spriteHalfWidth = spriteWidth / 2;
            float spriteHalfHeight = spriteHeight / 2;
            List<Vector2> listTopPoint = new List<Vector2>();
            List<Vector2> listRightPoint = new List<Vector2>();
            List<Vector2> listBotPoint = new List<Vector2>();
            List<Vector2> listLeftPoint = new List<Vector2>();
            for (var y = 0; y < Height; ++y)
            {
                for (var x = 0; x < Width; ++x)
                {
                    var idx = x + (y * Width);
                    // bỏ qua hole
                    if (levelData.Tiles[idx].TileType == TileType.Hole)
                        continue;
                    
                    // nếu ô bên trái ngoài viền hoặc trống
                    if (!IsXInside(x - 1) || IsHole(x - 1, y))
                    {
                        // nếu ô bên trên ngoài viền hoặc trống
                        if (!IsYInside(y - 1) || IsHole(x, y - 1))
                        {
                            // add node top left
                            listTopPoint.Add(new Vector2(TilePositions[idx].x - spriteHalfWidth, TilePositions[idx].y + spriteHalfHeight) * 10);
                            // nếu ô bên phải của ô bên trên không ngoài viền và không trống
                            if (IsXInside(x + 1) && IsYInside(y - 1) && !IsHole(x + 1, y - 1))
                            {
                                // add node top right
                                listTopPoint.Add(new Vector2(TilePositions[idx].x + spriteHalfWidth, TilePositions[idx].y + spriteHalfHeight) * 10);
                            }
                        }
                        // nếu ô bên dưới ngoài viền hoặc trống
                        if (!IsYInside(y + 1) || IsHole(x, y + 1))
                        {
                            // add node bot left
                            listBotPoint.Add(new Vector2(TilePositions[idx].x - spriteHalfWidth, TilePositions[idx].y - spriteHalfHeight) * 10);
                            // nếu ô bên phải của ô bên dưới không ngoài viền và không trống
                            if (IsXInside(x + 1) && IsYInside(y + 1) && !IsHole(x + 1, y + 1))
                            {
                                // add node bot right
                                listBotPoint.Add(new Vector2(TilePositions[idx].x + spriteHalfWidth, TilePositions[idx].y - spriteHalfHeight) * 10);
                            }
                        }
                    }
                    
                    // nếu ô bên phải ngoài viền hoặc trống
                    if (!IsXInside(x + 1) || IsHole(x + 1, y))
                    {
                        // nếu ô bên trên ngoài viền hoặc trống
                        if (!IsYInside(y - 1) || IsHole(x, y - 1))
                        {
                            // add node top right
                            listTopPoint.Add(new Vector2(TilePositions[idx].x + spriteHalfWidth, TilePositions[idx].y + spriteHalfHeight) * 10);
                            // nếu ô bên trái của ô bên trên không ngoài viền và không trống
                            if (IsXInside(x - 1) && IsYInside(y - 1) && !IsHole(x - 1, y - 1))
                            {
                                // add node top left
                                listTopPoint.Add(new Vector2(TilePositions[idx].x - spriteHalfWidth, TilePositions[idx].y + spriteHalfHeight) * 10);
                            }
                        }
                        // nếu ô bên dưới ngoài viền hoặc trống
                        if (!IsYInside(y + 1) || IsHole(x, y + 1))
                        {
                            // bot right
                            listBotPoint.Add(new Vector2(TilePositions[idx].x + spriteHalfWidth, TilePositions[idx].y - spriteHalfHeight) * 10);
                            // nếu ô bên trái của ô bên dưới không ngoài viền và không trống
                            if (IsXInside(x - 1) && IsYInside(y + 1) && !IsHole(x - 1, y + 1))
                            {
                                // add node bot right
                                listBotPoint.Add(new Vector2(TilePositions[idx].x - spriteHalfWidth, TilePositions[idx].y - spriteHalfHeight) * 10);
                            }
                        }
                    }
                }
            }
            listTopPoint.Sort(((a, b) => a.x.CompareTo(b.x)));
            for (int i = 0; i < listTopPoint.Count; i++)
            {
                if(i == listTopPoint.Count - 1) continue;
                if (Math.Abs(listTopPoint[i].x - listTopPoint[i + 1].x) < 0.1f)
                {
                    if(i - 1 < 0) continue;
                    if (Math.Abs(listTopPoint[i - 1].y - listTopPoint[i].y) > 0.1f)
                    {
                        Vector2 cache = listTopPoint[i];
                        listTopPoint[i]= listTopPoint[i + 1];
                        listTopPoint[i + 1] = cache;
                    } 
                }
            }
            listTopPoint.Reverse();
            listBotPoint.Sort(((a, b) => a.x.CompareTo(b.x)));
            for (int i = 0; i < listBotPoint.Count; i++)
            {
                if(i == listBotPoint.Count - 1) continue;
                if (Math.Abs(listBotPoint[i].x - listBotPoint[i + 1].x) < 0.1f)
                {
                    if(i - 1 < 0) continue;
                    if (Math.Abs(listBotPoint[i - 1].y - listBotPoint[i].y) > 0.1f)
                    {
                        Vector2 cache = listBotPoint[i];
                        listBotPoint[i]= listBotPoint[i + 1];
                        listBotPoint[i + 1] = cache;
                    } 
                }
            }
            
            Ferr2DT_PathTerrain terrain = tilePools.BorderTerrain;
            terrain.ClearPoints();
            
            for (int i = 0; i < listTopPoint.Count; i++)
            {
                Debug.Log(listTopPoint[i]);
                terrain.AddPoint(listTopPoint[i]);
            }

            for (int i = 0; i < listBotPoint.Count; i++)
            {
                terrain.AddPoint(listBotPoint[i]);
            }
            terrain.Build();
        }

        private bool IsHole(int x, int y)
        {
            return levelData.Tiles[x + y * Width].TileType == TileType.Hole;
        }

        private bool IsXInside(int x)
        {
            return x >= 0 && x < Width;
        }

        private bool IsYInside(int y)
        {
            return y >= 0 && y < Height;
        }
        
        private void CreateBorderBackground()
        {   
            for (var height = 0; height < Height; ++height)
            {
                for (var width = 0; width < Width; ++width)
                {
                    var idx = width + (height * Width);
                    if (width == 0 && height == 0) // top left
                    {
                        SetPosition(tilePools.GetTopLeftBorder(), TilePositions[idx]);   
                    }
                    else if (width == Width - 1 && height == 0) // top right
                    {
                        SetPosition(tilePools.GetTopRightBorder(), TilePositions[idx]);
                    }
                    else if (width == Width - 1 && height == Height - 1) // bot right
                    {
                        SetPosition(tilePools.GetBotRightBorder(), TilePositions[idx]);
                    }
                    else if (width == 0 && height == Height - 1) // bot left
                    {
                        SetPosition(tilePools.GetBotLeftBorder(), TilePositions[idx]);
                    }
                    else if (height == 0) // top
                    {
                        SetPosition(tilePools.GetTopBorder(), TilePositions[idx]);
                    }
                    else if (height == Height - 1) // bot
                    {
                        SetPosition(tilePools.GetBotBorder(), TilePositions[idx]);
                    }
                    else if (width == 0) // left
                    {
                        SetPosition(tilePools.GetLeftBorder(), TilePositions[idx]);
                    }
                    else if (width == Width - 1) // right
                    {
                        SetPosition(tilePools.GetRightBorder(), TilePositions[idx]);
                    }
                }
            }
        }

        private void SetPosition(GameObject go, Vector3 pos)
        {
            go.transform.position = pos;
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
            mainCamera.orthographicSize = total * zoomLevel * (Screen.height / (float)Screen.width) * 0.5f;
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
