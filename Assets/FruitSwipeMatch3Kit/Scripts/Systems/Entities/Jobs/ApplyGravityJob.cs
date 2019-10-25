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
           // BottomLeft();
        }

        private void Bottom()
        {
            for (var i = 0; i < Width; i++)
            {
                for (var j = Height - 1; j >= 0; j--)
                {
                    var idx = i + j * Width;
                    var tile = Tiles[idx];
                    if (tile == Entity.Null || HoleSlotData.Exists(tile))
                        continue;
                    if (tile != Entity.Null && BlockerData.Exists(tile) &&
                            BlockerData[tile].Type != BlockerType.Wood &&
                            BlockerData[tile].Type != BlockerType.Wood2 &&
                            BlockerData[tile].Type != BlockerType.Wood3)
                        continue;
                    // Find bottom.
                    var bottom = -1;
                    for (var k = j + 1; k < Height; k++)
                    {
                        var idx2 = i + k * Width;
                        var bottomTile = Tiles[idx2];
                        if (bottomTile == Entity.Null && !HoleSlotData.Exists(bottomTile))
                            bottom = k;
                        else if (bottomTile != Entity.Null && BlockerData.Exists(bottomTile))
                            break;
                    }
                    
                    if (bottom == -1)
                        continue;

                    if (tile == Entity.Null)
                        continue;

                    // Move down any tiles above empty spaces.
                    var numTilesToFall = bottom - j;
                    Tiles[idx + (numTilesToFall * Width)] = tile;

                    var tilePos = TilePosition[tile];
                    Ecb.SetComponent(tile, new TilePosition
                    {
                        X = tilePos.X,
                        Y = tilePos.Y + numTilesToFall,
                    });
                    
                    var translationData = TranslationData[tile];
                    Ecb.SetComponent(tile, new Translation
                    {
                        Value =
                        {
                            x = translationData.Value.x,
                            y = translationData.Value.y - numTilesToFall * SpriteHeight,
                            z = translationData.Value.z
                        }
                    });

                    Ecb.AddComponent(tile, new GravityTag());
                    
                    Tiles[idx] = Entity.Null;
                }
            }
        }

        private void BottomLeft()
        {
            for (var i = Width - 1; i >= 0; i--)
            {
                int lastHeight = Height - 1;
                for (var j = lastHeight; j >= 0; j--)
                {
                    var idx = i + j * Width;
                    var tile = Tiles[idx];
                    if (tile == Entity.Null ||
                        BlockerData.Exists(tile) ||
                        HoleSlotData.Exists(tile))
                        continue;

                    // Find bottom left.
                    var bottom = j + 1;
                    var left = i - 1;
                    if(left < 0 || bottom >= Height) continue;
                    var idxBottomLeft = left + bottom * Width;
                    var bottomLeftTile = Tiles[idxBottomLeft];
                    if (bottomLeftTile == Entity.Null && !HoleSlotData.Exists(bottomLeftTile))
                    {
                        Debug.Log(idxBottomLeft);
                        // Move to bottom left
                        Tiles[idxBottomLeft] = tile;

                        var tilePos = TilePosition[tile];
                        Ecb.SetComponent(tile, new TilePosition
                        {
                            X = tilePos.X - 1,
                            Y = tilePos.Y + 1,
                        });
                        Debug.Log(tilePos.X - 1 + " " + tilePos.Y + 1);
                        var translationData = TranslationData[tile];
                        Ecb.SetComponent(tile, new Translation
                        {
                            Value =
                            {
                                x = translationData.Value.x - SpriteWidth,
                                y = translationData.Value.y - SpriteHeight,
                                z = translationData.Value.z
                            }
                        });

                        Ecb.AddComponent(tile, new GravityTag());

                        Tiles[idx] = Entity.Null;
                    }
                }
            }
        }

        private void BottomRight()
        {
            for (var i = 0; i < Width; i++)
            {
                for (var j = Height - 1; j >= 0; j--)
                {
                    var idx = i + j * Width;
                    var tile = Tiles[idx];
                    if (tile == Entity.Null ||
                        BlockerData.Exists(tile) ||
                        HoleSlotData.Exists(tile))
                        continue;

                    // Find bottom.
                    var bottom = -1;
                    for (var k = j; k < Height; k++)
                    {
                        var idx2 = i + k * Width;
                        var bottomTile = Tiles[idx2];
                        if (bottomTile == Entity.Null && !HoleSlotData.Exists(bottomTile))
                            bottom = k;
                        else if (bottomTile != Entity.Null && BlockerData.Exists(bottomTile))
                            break;
                    }
                    
                    if (bottom == -1)
                        continue;

                    if (tile == Entity.Null)
                        continue;

                    // Move down any tiles above empty spaces.
                    var numTilesToFall = bottom - j;
                    Tiles[idx + (numTilesToFall * Width)] = tile;

                    var tilePos = TilePosition[tile];
                    Ecb.SetComponent(tile, new TilePosition
                    {
                        X = tilePos.X,
                        Y = tilePos.Y + numTilesToFall,
                    });

                    var translationData = TranslationData[tile];
                    Ecb.SetComponent(tile, new Translation
                    {
                        Value =
                        {
                            x = translationData.Value.x,
                            y = translationData.Value.y - numTilesToFall * SpriteHeight,
                            z = translationData.Value.z
                        }
                    });

                    Ecb.AddComponent(tile, new GravityTag());

                    Tiles[idx] = Entity.Null;
                }
            }
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
                        if (Tiles[pResultFind.x] == Entity.Null)
                        {
                            Debug.Log("null");
                        }
                        Tiles[idx] = Tiles[pResultFind.x];
                        var tilePos = TilePosition[Tiles[pResultFind.x]];
                        var newPos = new TilePosition
                        {
                            X = tilePos.X - numTilesToMove,
                            Y = tilePos.Y + numTilesToFall,
                        };
                        Ecb.SetComponent(Tiles[pResultFind.x], newPos);
                        TilePosition[Tiles[pResultFind.x]] = newPos;
                        var translationData = TranslationData[Tiles[pResultFind.x]];
                        translationData = new Translation
                            {
                                Value =
                                {
                                    x = translationData.Value.x - numTilesToMove * SpriteWidth,
                                    y = translationData.Value.y - numTilesToFall * SpriteHeight,
                                    z = translationData.Value.z
                                }
                            }
                            ;
                        Ecb.SetComponent(Tiles[pResultFind.x], translationData);
                        TranslationData[Tiles[pResultFind.x]] = translationData;
                        Ecb.AddComponent(Tiles[pResultFind.x], new GravityTag());

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
                    
                    if ((HoleSlotData.Exists(tile) || (BlockerData.Exists(tile) &&
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
