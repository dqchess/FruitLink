// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store EULA,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using DG.Tweening;
using Unity.Collections;
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
        private EntityArchetype gravityFinishedArchetype;
        private EntityArchetype boosterResolutionArchetype;
        private static readonly int Falling = Animator.StringToHash("Falling");

        protected override void OnCreate()
        {
            Enabled = false;
            gravityFinishedArchetype = EntityManager.CreateArchetype(typeof(GravityFinishedEvent));
            boosterResolutionArchetype = EntityManager.CreateArchetype(
                typeof(ResolveBoostersData));
        }

        protected override void OnUpdate()
        {
            bool pending = false;
            var pQuery = Entities.WithAny<ApplyGravityData>().WithAny<FillEmptySlotsData>();
            pQuery.ForEach((Entity entity) => { pending = true; });
            if (pending)
            {
                return;
            }
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
                seq.AppendInterval(GameplayConstants.FallingExistingTilesSpeed+0.05f);
                seq.AppendCallback(OnGravityCompleted);
                Entities.WithAll<CheckGravityTag>().ForEach(entity =>
                {
                    PostUpdateCommands.DestroyEntity(entity);
                });
            }
            else
            {
                bool isCompleted = false;
                Entities.WithAll<CheckGravityTag>().ForEach(entity =>
                {
                    isCompleted = true;
                    PostUpdateCommands.DestroyEntity(entity);
                });
                if (isCompleted)
                {
                    OnGravityCompleted();
                }
            }
        }

        private void OnGravityCompleted()
        {
            var pendingBoosterGroup = GetEntityQuery(typeof(PendingBoosterData));
            var levelHasPendingBoosters = pendingBoosterGroup.CalculateLength() > 0;
            var inputSystem = World.GetExistingSystem<PlayerInputSystem>();
            
            if (!inputSystem.IsBoosterExploding() &&
                !inputSystem.IsBoosterChainResolving() &&
                !levelHasPendingBoosters)
            {
                if (inputSystem.PendingBoosterTiles.Count > 0)
                {
                    var pendingBooster = inputSystem.PendingBoosterTiles[0];
                    inputSystem.PendingBoosterTiles.RemoveAt(0);
                    EntityManager.AddComponentData(pendingBooster, new PendingBoosterData());
                    EntityManager.CreateEntity(boosterResolutionArchetype);
                }
                else
                {
                    inputSystem.UnlockInput();
                    EntityManager.CreateEntity(gravityFinishedArchetype);   
                }
            }

            if (levelHasPendingBoosters)
                EntityManager.CreateEntity(boosterResolutionArchetype);
        }
    }
}
