using System.Collections.Generic;
using DG.Tweening;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace FruitSwipeMatch3Kit
{
    [UpdateAfter(typeof(PossibleMoveSystem))]
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
                    GameState.SwapCount++;
                    Swap();
                    EntityManager.CreateEntity(ComponentType.ReadOnly<CheckPossibleMoveTag>());
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

            List<int> cachePossibleIdx = new List<int>(possibleIdx);
            for (int i = 0; i < cachePossibleIdx.Count; i++)
            {
                int startIndex = cachePossibleIdx[i];
                int randomIndex = possibleIdx[Random.Range(0, possibleIdx.Count)];
                possibleIdx.Remove(randomIndex);
                MoveEntity(tileEntities, startIndex, randomIndex);
                MoveAnimation(tileGos[startIndex], tilePositions[randomIndex]);
                MoveGameObject(tileGos, startIndex, randomIndex);
            }
        }

        private void MoveAnimation(GameObject tileGo, float3 endPos)
        {
            tileGo.transform.DOMove(endPos, GameplayConstants.FallingExistingTilesSpeed);
//                .OnComplete(() =>
//            {
//                var anim = go.GetComponent<Animator>();
//                if(anim != null) anim.SetTrigger(Falling);
//                var fallingTile = go.GetComponent<IFallingTile>();
//                fallingTile?.OnTileFell();
//            });
        }

        private void MoveEntity(NativeArray<Entity> tileEntities, int fromIdx, int toIdx)
        {
            var cacheEntity = tileEntities[fromIdx];
            tileEntities[fromIdx] = tileEntities[toIdx];
            tileEntities[toIdx] = cacheEntity;
        }

        private void MoveGameObject(List<GameObject> tileGos, int fromIdx, int toIdx)
        {
            GameObject startObject = tileGos[fromIdx];
            tileGos[fromIdx] = tileGos[toIdx];
            tileGos[toIdx] = startObject;
        }
    }
}