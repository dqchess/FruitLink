// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace FruitSwipeMatch3Kit
{
    /// <summary>
    /// This system resolves the bomb power-up.
    /// </summary>
    [AlwaysUpdateSystem]
    public class BombPowerupResolutionSystem : PowerupResolutionSystem
    {
        protected override void OnCreate()
        {
            base.OnCreate();
            
            query = GetEntityQuery(
                ComponentType.ReadOnly<ResolveBombPowerupEvent>());
        }

        protected override void ResolvePowerup()
        {
            if (!Input.GetMouseButtonDown(0))
                return;
            
            var mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            if (Physics2D.RaycastNonAlloc(mousePos, Vector3.forward, raycastResults, Mathf.Infinity, tileLayer) > 0)
            {
                var result = raycastResults[0];
                var tile = result.collider.gameObject.GetComponent<Tile>();
                if (tile != null)
                {
                    var goe = result.collider.GetComponent<GameObjectEntity>();
                    var entity = goe.Entity;
                    var tilePos = goe.EntityManager.GetComponentData<TilePosition>(entity);
                    var particlePools = Object.FindObjectOfType<ParticlePools>();

                    var width = levelCreationSystem.Width;
                    var height = levelCreationSystem.Height;

                    var tilesToDestroy = new List<int>(9);
                    
                    var x = tilePos.X;
                    var y = tilePos.Y;
                    AddTileToDestroyIfValid(x, y, tilesToDestroy, width, height);
                    AddTileToDestroyIfValid(x - 1, y, tilesToDestroy, width, height);
                    AddTileToDestroyIfValid(x + 1, y, tilesToDestroy, width, height);
                    
                    AddTileToDestroyIfValid(x - 1, y - 1, tilesToDestroy, width, height);
                    AddTileToDestroyIfValid(x, y - 1, tilesToDestroy, width, height);
                    AddTileToDestroyIfValid(x + 1, y - 1, tilesToDestroy, width, height);
                    
                    AddTileToDestroyIfValid(x - 1, y + 1, tilesToDestroy, width, height);
                    AddTileToDestroyIfValid(x, y + 1, tilesToDestroy, width, height);
                    AddTileToDestroyIfValid(x + 1, y + 1, tilesToDestroy, width, height);
                   
                    TileUtils.DestroyTiles(
                        tilesToDestroy,
                        levelCreationSystem.TileEntities,
                        levelCreationSystem.TileGos,
                        levelCreationSystem.Slots,
                        particlePools,
                        width,
                        height);

                    OnResolvedPowerup();
                }
            }
        }

        private void AddTileToDestroyIfValid(int x, int y, List<int> tiles, int width, int height)
        {
            if (x >= 0 && x < width && y >= 0 && y < height)
            {
                var idx = x + y * width;
                tiles.Add(idx);
            }
        }
    }
}

