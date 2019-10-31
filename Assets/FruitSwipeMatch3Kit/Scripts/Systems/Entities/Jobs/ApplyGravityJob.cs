// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace FruitSwipeMatch3Kit
{
    /// <summary>
    /// Internal job used by the gravity system.
    /// </summary>
    public struct ApplyGravityJob : IJob
    {
        public EntityCommandBuffer Ecb;
        public NativeArray<Entity> Tiles;
        public ComponentDataFromEntity<TilePosition> TilePosition;
        public ComponentDataFromEntity<Translation> TranslationData;
        [ReadOnly] public int Width;
        [ReadOnly] public int Height;
        [ReadOnly] public float SpriteWidth;
        [ReadOnly] public float SpriteHeight;
        [ReadOnly] public ComponentDataFromEntity<HoleSlotData> HoleSlotData;
        [ReadOnly] public ComponentDataFromEntity<BlockerData> BlockerData;

        public void Execute()
        {
            ResortTiles();
        }
        
        private void ResortTiles()
        {
            bool dirty = false;
            do
            {
                dirty = false;
                for (var i = 0; i < Width; i++)
                {
                    for (var j = Height - 1; j >= 0; j--)
                    {
                        var idx = i + j * Width;
                        var tile = Tiles[idx];
                        int2 pResultFind = new int2(-1, 0);
                        if (tile == Entity.Null)
                            pResultFind = FindTileForEmptySlot(i, j, 0);


                        if (pResultFind.x == -1)
                            continue;


                        // Move down any tiles above empty spaces.
                        var numTilesToFall = j - (pResultFind.x / Width);
                        var numTilesToMove = (pResultFind.x % Width) - i;
                        Tiles[idx] = Tiles[pResultFind.x];
                        if (TilePosition.Exists(Tiles[idx]))
                        {
                            var tilePos = TilePosition[Tiles[pResultFind.x]];
                            var newPos = new TilePosition
                            {
                                X = tilePos.X - numTilesToMove,
                                Y = tilePos.Y + numTilesToFall,
                            };
                            Ecb.SetComponent(Tiles[pResultFind.x], newPos);
                            TilePosition[Tiles[pResultFind.x]] = newPos;                          
                        }

                        if (TranslationData.Exists(Tiles[idx]))
                        {
                            var translationData = TranslationData[Tiles[pResultFind.x]];
                            translationData = new Translation
                            {
                                Value =
                                {
                                    x = translationData.Value.x - numTilesToMove * SpriteWidth,
                                    y = translationData.Value.y - numTilesToFall * SpriteHeight,
                                    z = translationData.Value.z
                                }
                            };
                            Ecb.SetComponent(Tiles[pResultFind.x], translationData);
                            TranslationData[Tiles[pResultFind.x]] = translationData;
                            Ecb.AddComponent(Tiles[pResultFind.x], new GravityTag());
                        }
                        
                        Tiles[pResultFind.x] = Entity.Null;
                        dirty = true;
                    }
                }
            } while (dirty);

        }

        private int2 FindTileForEmptySlot(int posX, int posY, int currentLength)
        {
            if (posX < 0 || posX >= Width)
            {
                return new int2(-1, currentLength);
            }
            var idxSelf = posX + posY * Width;
            var tileSelf = Tiles[idxSelf];
            if (tileSelf == Entity.Null)
            {
                int pSide = 1;
                for (int row = posY ; row <= posY || pSide == 1; row-=pSide)
                {
                    if (row < 0)
                    {
                        return new int2(-1, currentLength);
                    } else if (pSide == -1 && row == posY)
                    {
                        return new int2(-1, currentLength);
                    }
                    currentLength++;
                    var idx = posX + row * Width;
                    var tile = Tiles[idx];
                    if (tile == Entity.Null && pSide == 1)
                        continue;
                    if (HoleSlotData.Exists(tile))
                    {
                        continue;
                    }
                    if ((  (BlockerData.Exists(tile) &&
                                                      BlockerData[tile].Type != BlockerType.Wood &&
                                                      BlockerData[tile].Type != BlockerType.Wood2 &&
                                                      BlockerData[tile].Type != BlockerType.Wood3))|| pSide ==-1)
                    {
                        int2 pResultLeft = FindTileForEmptySlot(posX - 1, row, currentLength);
                        int2 pResultRight = FindTileForEmptySlot(posX + 1, row, currentLength);
                        if (pResultLeft.x != -1 && (pResultLeft.y < pResultRight.y || pResultRight.x == -1))
                        {
                            return new int2(pResultLeft.x, currentLength + pResultLeft.y);
                        }
                        else if (pResultRight.x != -1)
                        {
                            return new int2(pResultRight.x, currentLength + pResultLeft.y);
                        }

                        if (pSide == 1)
                        {
                            pSide = -1;
                            continue;
                        }else if (pSide == -1 )
                        {
                            continue;
                        }
                    }
                    return new int2(idx, currentLength);
                }

                return new int2(-1, currentLength);
            }
            else if (HoleSlotData.Exists(tileSelf) || (BlockerData.Exists(tileSelf) &&
                                                       BlockerData[tileSelf].Type != BlockerType.Wood &&
                                                       BlockerData[tileSelf].Type != BlockerType.Wood2 &&
                                                       BlockerData[tileSelf].Type != BlockerType.Wood3))
            {
                return new int2(-1, currentLength);
            }
            else
            {
                return new int2(idxSelf, currentLength);
            }
        }
    }
}
