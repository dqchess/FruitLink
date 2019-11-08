// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;
using DG.Tweening;
using Unity.Entities;
using UnityEngine;

namespace FruitSwipeMatch3Kit
{
    /// <summary>
    /// This system resolves the color bomb power-up.
    /// </summary>
    [AlwaysUpdateSystem]
    public class ColorBombPowerupResolutionSystem : PowerupResolutionSystem
    {
        protected override void OnCreate()
        {
            base.OnCreate();
            
            query = GetEntityQuery(
                ComponentType.ReadOnly<ResolveColorBombPowerupEvent>());
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
                    var particlePools = Object.FindObjectOfType<ParticlePools>();

                    var width = levelCreationSystem.Width;
                    var height = levelCreationSystem.Height;

                    var tilesToDestroy = new List<int>(16);
                    
                    var selectedColor = EntityManager.GetComponentData<TileData>(entity).Type;
                    foreach (var tileEntity in levelCreationSystem.TileEntities)
                    {
                        if (tileEntity != Entity.Null && EntityManager.HasComponent<TileData>(tileEntity))
                        {
                            var newColor = EntityManager.GetComponentData<TileData>(tileEntity).Type;
                            if (newColor == selectedColor)
                            {
                                var tilePos = EntityManager.GetComponentData<TilePosition>(tileEntity);
                                tilesToDestroy.Add(tilePos.X + tilePos.Y * width);
                            }
                        }
                    }
                    
                    bool isBoosterExploding = false;
                    var selectedBooster = Entity.Null;
                    
                    for (int i = 0; i < tilesToDestroy.Count; i++)
                    {
                        int idx = tilesToDestroy[i];
                        entity = levelCreationSystem.TileEntities[idx];

                        if (EntityManager.HasComponent<BoosterData>(entity))
                        {
                            if (selectedBooster == Entity.Null)
                            {
                                selectedBooster = entity;
                                isBoosterExploding = true;
                                EntityManager.AddComponentData(entity, new PendingBoosterData());
                                tilesToDestroy.RemoveAt(i);
                                i--;
                            }
                            else
                            {
                                inputSystem.PendingBoosterTiles.Add(entity);
                                tilesToDestroy.RemoveAt(i);
                                i--;
                            }
                        }
                    }
                    
                    var seg = DOTween.Sequence();
                    seg.AppendInterval(GameplayConstants.FallingExistingTilesSpeed);
                    seg.AppendCallback(() =>
                    {
                        TileUtils.DestroyTiles(
                            tilesToDestroy,
                            levelCreationSystem.TileEntities,
                            levelCreationSystem.TileGos,
                            levelCreationSystem.Slots,
                            particlePools,
                            width,
                            height);
                        if(isBoosterExploding)
                        {
                            seg = DOTween.Sequence();
                            seg.AppendInterval(GameplayConstants.FallingExistingTilesSpeed);
                            seg.AppendCallback(() =>
                            {
                                var e = EntityManager.CreateEntity();
                                EntityManager.AddComponentData(e, new ResolveBoostersData());
                            });
                        }

                        OnResolvedPowerup();
                    });
                    if (GameState.IsTutorial)
                    {
                        seg = DOTween.Sequence();
                        seg.AppendInterval(GameplayConstants.FallingExistingTilesSpeed);
                        seg.AppendCallback(() =>
                        {
                            var tutorial = World.GetExistingSystem<TutorialSystem>();
                            tutorial.Destroy();
                            tutorial.ClearDarkTile();
                        });
                    }
                }
            }
        }
    }
}


