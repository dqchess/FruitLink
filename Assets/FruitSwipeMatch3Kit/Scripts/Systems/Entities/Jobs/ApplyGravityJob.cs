// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

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
        [ReadOnly] public float SpriteHeight;
        [ReadOnly] public ComponentDataFromEntity<HoleSlotData> HoleSlotData;
        [ReadOnly] public ComponentDataFromEntity<BlockerData> BlockerData;

        public void Execute()
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
