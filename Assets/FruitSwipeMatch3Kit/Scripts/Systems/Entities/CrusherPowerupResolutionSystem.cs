// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using Unity.Entities;
using UnityEngine;

namespace FruitSwipeMatch3Kit
{
    /// <summary>
    /// This system resolves the crusher power-up.
    /// </summary>
    [AlwaysUpdateSystem]
    public class CrusherPowerupResolutionSystem : PowerupResolutionSystem
    {
        protected override void OnCreate()
        {
            base.OnCreate();
            
            query = GetEntityQuery(
                ComponentType.ReadOnly<ResolveCrusherPowerupEvent>());
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
                    var idx = tilePos.X + tilePos.Y * levelCreationSystem.Width;
                    TileUtils.DestroyTile(
                        idx,
                        levelCreationSystem.TileEntities,
                        levelCreationSystem.TileGos,
                        levelCreationSystem.Slots,
                        particlePools,
                        true);

                    OnResolvedPowerup();
                }
            }
            else if (Physics2D.RaycastNonAlloc(mousePos, Vector3.forward, raycastResults, Mathf.Infinity,
                         slotLayer) > 0)
            {
                var result = raycastResults[0];
                var slot = result.collider.gameObject.GetComponent<Slot>();
                if (slot != null)
                {
                    var particlePools = Object.FindObjectOfType<ParticlePools>();
                    TileUtils.DestroySlot(
                        slot.gameObject,
                        levelCreationSystem.Slots,
                        levelCreationSystem.TilePositions,
                        particlePools);

                    OnResolvedPowerup();
                }
            }
            else if (Physics2D.RaycastNonAlloc(mousePos, Vector3.forward, raycastResults, Mathf.Infinity,
                         blockerLayer) > 0)
            {
                var result = raycastResults[0];
                var blocker = result.collider.gameObject.GetComponent<Blocker>();
                if (blocker != null)
                {
                    var goe = result.collider.GetComponent<GameObjectEntity>();
                    var entity = goe.Entity;
                    var tilePos = goe.EntityManager.GetComponentData<TilePosition>(entity);
                    var particlePools = Object.FindObjectOfType<ParticlePools>();
                    var idx = tilePos.X + tilePos.Y * levelCreationSystem.Width;
                    TileUtils.DestroyBlocker(
                        idx,
                        levelCreationSystem.TileEntities,
                        levelCreationSystem.TileGos,
                        particlePools);

                    OnResolvedPowerup();
                }
            }
        }
    }
}
