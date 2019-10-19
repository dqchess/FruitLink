// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
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
        bool isFall;
        
        public void Execute()
        {
            Bottom();
//            BottomLeft();
            if (!isFall)
            {
                var inputSystem = World.Active.GetExistingSystem<PlayerInputSystem>();
                if (!inputSystem.IsBoosterExploding() && !inputSystem.IsBoosterChainResolving())
                    inputSystem.UnlockInput();
            }
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
                            BlockerData[tile].Type != BlockerType.Wood1 &&
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

                    isFall = true;
                }
            }
        }

        private void BottomLeft()
        {
            for (var i = Width - 1; i >= 0; i--)
            {
                for (var j = Height - 1; j >= 0; j--)
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
    }
}
