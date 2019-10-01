// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using DG.Tweening;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace FruitSwipeMatch3Kit
{
    /// <summary>
    /// This system resolves the swap power-up.
    /// </summary>
    [AlwaysUpdateSystem]
    public class SwapPowerupResolutionSystem : PowerupResolutionSystem
    {
        private bool isDragging;
        
        private GameObject tileA;
        private GameObject tileB;
        
        protected override void OnCreate()
        {
            base.OnCreate();
            
            query = GetEntityQuery(
                ComponentType.ReadOnly<ResolveSwapPowerupEvent>());
        }
        
        protected override void ResolvePowerup()
        {
            if (Input.GetMouseButtonDown(0))
            {
                isDragging = true;

                var mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                if (Physics2D.RaycastNonAlloc(mousePos, Vector3.forward, raycastResults, Mathf.Infinity, tileLayer) > 0)
                {
                    var result = raycastResults[0];
                    var tile = result.collider.gameObject;
                    if (tile != null)
                    {
                        tileA = tile;
                    }
                }
            }

            if (Input.GetMouseButtonUp(0))
                isDragging = false;

            if (isDragging && tileA != null)
            {
                var mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                if (Physics2D.RaycastNonAlloc(mousePos, Vector3.forward, raycastResults, Mathf.Infinity, tileLayer) > 0)
                {
                    var result = raycastResults[0];
                    var tile = result.collider.gameObject;
                    if (tile != null && tile != tileA)
                    {
                        tileB = tile;

                        SwapTiles();
                    }
                }
            }
        }

        private void SwapTiles()
        {
            var tempPos = tileA.transform.position;
            var bPos = tileB.transform.position;

            var spriteA = tileA.GetComponent<SpriteRenderer>();
            var spriteB = tileB.GetComponent<SpriteRenderer>();
            var originalOrderA = spriteA.sortingOrder;
            var originalOrderB = spriteB.sortingOrder;
                
            spriteA.sortingOrder = 10;
            
            tileA.transform.DOMove(bPos, 0.3f);
            tileB.transform.DOMove(tempPos, 0.3f).OnComplete(() =>
            {
                spriteA.sortingOrder = originalOrderB;
                spriteB.sortingOrder = originalOrderA;
            });

            var entityA = tileA.GetComponent<GameObjectEntity>().Entity;
            var entityB = tileB.GetComponent<GameObjectEntity>().Entity;

            var posA = EntityManager.GetComponentData<TilePosition>(entityA);
            var posB = EntityManager.GetComponentData<TilePosition>(entityB);

            var translationA = EntityManager.GetComponentData<Translation>(entityA);
            var translationB = EntityManager.GetComponentData<Translation>(entityB);

            var width = levelCreationSystem.Width;
            var idxA = posA.X + posA.Y * width;
            var idxB = posB.X + posB.Y * width;

            var tileEntities = levelCreationSystem.TileEntities;
            tileEntities[idxA] = entityB;
            tileEntities[idxB] = entityA;

            var tileGos = levelCreationSystem.TileGos;
            tileGos[idxA] = tileB;
            tileGos[idxB] = tileA;

            EntityManager.SetComponentData(entityA, posB);
            EntityManager.SetComponentData(entityB, posA);

            EntityManager.SetComponentData(entityA, translationB);
            EntityManager.SetComponentData(entityB, translationA);

            OnResolvedPowerup();
            tileA = null;
            tileB = null;
            isDragging = false;
        }
    }
}
