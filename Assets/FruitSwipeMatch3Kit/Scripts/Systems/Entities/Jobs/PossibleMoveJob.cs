using System.Collections.Generic;
using DG.Tweening;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace FruitSwipeMatch3Kit
{
    public struct PossibleMoveJob : IJob
    {
        public EntityCommandBuffer Ecb;
        public NativeArray<Entity> Tiles;
        [ReadOnly] public int Width;
        [ReadOnly] public int Height;
        [ReadOnly] public ComponentDataFromEntity<HoleSlotData> HoleSlotData;
        [ReadOnly] public ComponentDataFromEntity<BlockerData> BlockerData;
        [ReadOnly] public ComponentDataFromEntity<TileData> TileData;

        public void Execute()
        {
            GameState.SuggestSequence.Kill();
            List<int> possibleMove = new List<int>();
            for (var j = 0; j < Height; ++j)
            {
                for (var i = 0; i < Width; ++i)
                {
                    var idx = i + j * Width;
                    var tile = Tiles[idx];
                    if (tile == Entity.Null || HoleSlotData.Exists(tile) || BlockerData.Exists(tile))
                        continue;
                    if (CheckTile(idx, ref possibleMove) >= GameplayConstants.NumTilesNeededForMatch)
                    {
                        GameState.SwapCount = 0;
                        GameState.SuggestIndexes = possibleMove;
                        GameState.SuggestSequence = DOTween.Sequence();
                        GameState.SuggestSequence.AppendInterval(GameplayConstants.SuggetionDelay);
                        GameState.SuggestSequence.AppendCallback(() =>
                        {
                            World.Active.GetExistingSystem<PlayerInputSystem>().DisplaySuggetion(GameState.SuggestIndexes);
                        });
                        return;
                    }
                }
            }
            Ecb.AddComponent(Ecb.CreateEntity(), new SwapTileEvent());
        }

        private int CheckTile(int startIdx, ref List<int> possibleMove)
        {
            List<int> openSet = new List<int>();
            HashSet<int> closeSet = new HashSet<int>();
            possibleMove.Clear();
            openSet.Add(startIdx);
            possibleMove.Add(startIdx);
            if (!TileData.Exists(Tiles[startIdx])) return 0;
            var startColor = TileData[Tiles[startIdx]].Type;
            while (openSet.Count > 0)
            {
                int currentIdx = openSet[0];
                openSet.RemoveAt(0);
                closeSet.Add(currentIdx);

                List<int> neighbours = TileUtils.GetNeighbours(currentIdx, Tiles, Width, Height, true);
                for (int i = 0; i < neighbours.Count; i++)
                {
                    int neighbourIdx = neighbours[i];
                    if (closeSet.Contains(neighbourIdx)) continue;
                    if (TileData.Exists(Tiles[neighbourIdx]))
                    {
                        if (TileData[Tiles[neighbourIdx]].Type == startColor)
                        {
                            if (!openSet.Contains(neighbourIdx)) openSet.Add(neighbourIdx);
                            possibleMove.Add(neighbourIdx);
                        }
                    }

                    closeSet.Add(neighbourIdx);
                }
            }

            return possibleMove.Count;
        }
    }
}