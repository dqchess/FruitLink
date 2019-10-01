// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store EULA,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;
using UnityEngine;

namespace FruitSwipeMatch3Kit
{
    public class TilePools : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField]
        private List<ObjectPool> colorTilePools;
        [SerializeField]
        private List<ObjectPool> slotPools;
        [SerializeField]
        private List<ObjectPool> blockerPools;
        [SerializeField]
        private List<ObjectPool> collectiblePools;
        
        [SerializeField]
        private ObjectPool lightBackgroundTilePool;
        [SerializeField]
        private ObjectPool darkBackgroundTilePool;
#pragma warning restore 649

        public List<ObjectPool> RandomizedColorTilePools => randomizedColorTilePools;
        private readonly List<ObjectPool> randomizedColorTilePools = new List<ObjectPool>();

        public void Initialize(LevelData levelData)
        {
            foreach (var pool in colorTilePools)
                pool.Initialize();

            foreach (var pool in slotPools)
                if (pool != null)
                    pool.Initialize();
            
            foreach (var pool in blockerPools)
                if (pool != null)
                    pool.Initialize();
            
            foreach (var pool in collectiblePools)
                pool.Initialize();
            
            lightBackgroundTilePool.Initialize();
            darkBackgroundTilePool.Initialize();
            
            if (!PlayerPrefs.HasKey("num_available_colors"))
            {
                var randomColors = levelData.AvailableColors;
                randomColors.Shuffle();
                PlayerPrefs.SetInt("num_available_colors", randomColors.Count);
                for (var i = 0; i < randomColors.Count; i++)
                    PlayerPrefs.SetInt($"available_colors_{i}", (int)randomColors[i]);
            }

            var numColors = PlayerPrefs.GetInt("num_available_colors");
            var availableColors = new List<ColorTileType>();
            for (var i = 0; i < numColors; i++)
                availableColors.Add((ColorTileType)PlayerPrefs.GetInt($"available_colors_{i}"));
            foreach (var color in availableColors)
                randomizedColorTilePools.Add(colorTilePools[(int)color]);
        }

        public GameObject GetColorTile(ColorTileType type)
        {
            return colorTilePools[(int)type].GetObject();
        }

        public GameObject GetRandomColorTile()
        {
            var idx = Random.Range(0, randomizedColorTilePools.Count);
            return randomizedColorTilePools[idx].GetObject();
        }
        
        public GameObject GetRandomColorTile(RandomColorTileType type)
        {
            return randomizedColorTilePools[(int)type % randomizedColorTilePools.Count].GetObject();
        }
        
        public GameObject GetSlot(SlotType type)
        {
            return slotPools[(int)type].GetObject();
        }

        public GameObject GetBlocker(BlockerType type)
        {
            return blockerPools[(int)type].GetObject();
        }

        public GameObject GetCollectible(CollectibleType type)
        {
            return collectiblePools[(int)type].GetObject();
        }

        public GameObject GetLightBackgroundTile()
        {
            return lightBackgroundTilePool.GetObject();
        }
        
        public GameObject GetDarkBackgroundTile()
        {
            return darkBackgroundTilePool.GetObject();
        }
    }
}
