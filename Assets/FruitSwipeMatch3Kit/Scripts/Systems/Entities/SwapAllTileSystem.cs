using System.Collections.Generic;
using DG.Tweening;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

namespace FruitSwipeMatch3Kit
{
    [UpdateAfter(typeof(PossibleMoveSystem))]
    [UpdateBefore(typeof(MatchEndSystem))]
    public class SwapAllTileSystem : ComponentSystem
    {
        private EntityQuery query;
        
        protected override void OnCreate()
        {
            Enabled = false;
            query = GetEntityQuery(ComponentType.ReadOnly<SwapTileEvent>());
        }

        protected override void OnUpdate()
        {
            bool isSwap = false;
            Entities.With(query).ForEach(entity =>
            {
                isSwap = true;
                PostUpdateCommands.DestroyEntity(entity);
            });
            
            if (isSwap)
            {
                if (GameState.SwapCount >= GameplayConstants.GameOverSwapCount)
                {
                    EntityManager.CreateEntity(ComponentType.ReadOnly<GravityFinishedEvent>());
                }
                else
                {
                    GameState.IsSwapping = true;
                    var input = World.GetExistingSystem<PlayerInputSystem>();
                    input.GameScreen.OpenNoMoreMovePopup();
                    input.LockInput();
                    var seg = DOTween.Sequence();
                    seg.AppendInterval(GameplayConstants.NoMoreMoveDelay);
                    seg.AppendCallback(() =>
                    {
                        GameState.IsSwapping = false;
                        GameState.SwapCount++;
                        Swap();
                        EntityManager.CreateEntity(ComponentType.ReadOnly<CheckPossibleMoveTag>());
                        input.UnlockInput();
                    });
                }
            }
        }

        private void Swap()
        {
            var levelCreationSystem = World.GetExistingSystem<LevelCreationSystem>();
            var tileEntities = levelCreationSystem.TileEntities;
            var tileGos = levelCreationSystem.TileGos;
            var tilePositions = levelCreationSystem.TilePositions;
            var allTileData = GetComponentDataFromEntity<TileData>();
            var width = levelCreationSystem.Width;
            var height = levelCreationSystem.Height;
            // list index of all fruits
            List<int> possibleIdx = new List<int>();
            for (var j = 0; j < height; ++j)
            {
                for (var i = 0; i < width; ++i)
                {
                    int idx = i + j * width;
                    if (allTileData.Exists(tileEntities[idx]))
                    {
                        possibleIdx.Add(idx);
                    }
                }
            }

            // clone possibleIdx
            List<int> cachePossibleIdx = new List<int>(possibleIdx);
            HashSet<int> swapedSet = new HashSet<int>();
            for (int i = 0; i < cachePossibleIdx.Count; i++)
            {
                // current index
                int startIndex = cachePossibleIdx[i];
                if(swapedSet.Contains(startIndex)) continue;
                // get random possible index
                int randomIndex = possibleIdx[Random.Range(0, possibleIdx.Count)];
                possibleIdx.Remove(startIndex);
                possibleIdx.Remove(randomIndex);
                swapedSet.Add(randomIndex);
                if(startIndex == randomIndex) continue;
                SwapEntity(tileEntities, startIndex, randomIndex);
                MoveAnimation(tileGos[startIndex], tilePositions[randomIndex]);
                MoveAnimation(tileGos[randomIndex], tilePositions[startIndex]);
                SwapGameObject(tileGos, startIndex, randomIndex);
            }
        }

        private void MoveAnimation(GameObject tileGo, float3 endPos)
        {
            tileGo.transform.DOMove(endPos, GameplayConstants.FallingExistingTilesSpeed);
        }

        private void SwapEntity(NativeArray<Entity> tileEntities, int fromIdx, int toIdx)
        {
            var fromTilePos = EntityManager.GetComponentData<TilePosition>(tileEntities[fromIdx]);
            var targetTilePos = EntityManager.GetComponentData<TilePosition>(tileEntities[toIdx]);
            EntityManager.SetComponentData(tileEntities[fromIdx], targetTilePos);
            EntityManager.SetComponentData(tileEntities[toIdx], fromTilePos);
            var fromTranslation = EntityManager.GetComponentData<Translation>(tileEntities[fromIdx]).Value;
            var targetTranslation = EntityManager.GetComponentData<Translation>(tileEntities[toIdx]).Value;
            EntityManager.SetComponentData(tileEntities[fromIdx], new Translation { Value = targetTranslation });
            EntityManager.SetComponentData(tileEntities[toIdx], new Translation { Value = fromTranslation });
            var cacheEntity = tileEntities[fromIdx];
            tileEntities[fromIdx] = tileEntities[toIdx];
            tileEntities[toIdx] = cacheEntity;
        }

        private void SwapGameObject(List<GameObject> tileGos, int fromIdx, int toIdx)
        {
            GameObject startObject = tileGos[fromIdx];
            tileGos[fromIdx] = tileGos[toIdx];
            tileGos[toIdx] = startObject;
        }
    }
}