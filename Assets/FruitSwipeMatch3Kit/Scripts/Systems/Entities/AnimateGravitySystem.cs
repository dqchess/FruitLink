// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store EULA,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using DG.Tweening;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace FruitSwipeMatch3Kit
{
    /// <summary>
    /// This system is responsible for making the tile game objects fall to the ground.
    /// </summary>
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class AnimateGravitySystem : ComponentSystem
    {
        private EntityArchetype boosterResolutionArchetype;

        private static readonly int Falling = Animator.StringToHash("Falling");

        protected override void OnCreate()
        {
            Enabled = false;
            boosterResolutionArchetype = EntityManager.CreateArchetype(
                typeof(ResolveBoostersData));
        }

        protected override void OnUpdate()
        {
            var hasGravity = false;
            Entities.WithAll<GravityTag>().ForEach((Entity entity, Transform transform) =>
            {
                hasGravity = true;
                PostUpdateCommands.RemoveComponent<GravityTag>(entity);

                // Sync GO position with backing entity's position.
                var translation = EntityManager.GetComponentData<Translation>(entity);
                var go = transform.gameObject;
                go.transform.DOMove(
                    translation.Value,
                    GameplayConstants.FallingExistingTilesSpeed).OnComplete(() =>
                {
                    var anim = go.GetComponent<Animator>();
                    if(anim != null) anim.SetTrigger(Falling);
                    var fallingTile = go.GetComponent<IFallingTile>();
                    fallingTile?.OnTileFell();
                });

                // Update GO array.
                var levelSystem = World.GetExistingSystem<LevelCreationSystem>();
                var width = levelSystem.Width;
                var tiles = levelSystem.TileGos;
                var tilePos = EntityManager.GetComponentData<TilePosition>(entity);
                var idx = tilePos.X + (tilePos.Y * width);
                tiles[idx] = go;
            });

            if (hasGravity)
            {
                SoundPlayer.PlaySoundFx("TileFalling");

                var seq = DOTween.Sequence();
                seq.AppendInterval(GameplayConstants.FallingExistingTilesSpeed);
                seq.AppendCallback(() =>
                {
                    var levelSystem = World.GetExistingSystem<LevelCreationSystem>();
                    var tileEntities = levelSystem.TileEntities;
                    var levelHasPendingBoosters = false;
                    foreach (var tile in tileEntities)
                    {
                        if (EntityManager.HasComponent<PendingBoosterData>(tile))
                        {
                            levelHasPendingBoosters = true;
                            break;
                        }
                    }

                    if (levelHasPendingBoosters)
                        EntityManager.CreateEntity(boosterResolutionArchetype);

                    var inputSystem = World.GetExistingSystem<PlayerInputSystem>();
                    if (!inputSystem.IsBoosterExploding() &&
                        !inputSystem.IsBoosterChainResolving() &&
                        !levelHasPendingBoosters)
                    {
                        inputSystem.UnlockInput();
                        EntityManager.CreateEntity(typeof(GravityFinishedEvent));
                    }

                    if (!inputSystem.IsBoosterExploding() &&
                        inputSystem.PendingBoosterTiles.Count > 0)
                    {
                        var pendingBooster = inputSystem.PendingBoosterTiles[0];
                        inputSystem.PendingBoosterTiles.RemoveAt(0);
                        EntityManager.AddComponentData(pendingBooster, new PendingBoosterData());
                    }
                });
            }
        }
    }
}
