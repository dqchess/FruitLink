// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store EULA,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;
using UnityEngine;

namespace FruitSwipeMatch3Kit
{
    public class ParticlePools : MonoBehaviour
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
#pragma warning restore 649
        
        public void Initialize()
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
        }

        public GameObject GetColorTileParticles(ColorTileType type)
        {
            return colorTilePools[(int)type].GetObject();
        }
        
        public GameObject GetSlotParticles(SlotType type)
        {
            return slotPools[(int)type].GetObject();
        }
        
        public GameObject GetBlockerParticles(BlockerType type)
        {
            return blockerPools[(int)type].GetObject();
        }
        
        public GameObject GetCollectibleParticles(CollectibleType type)
        {
            return collectiblePools[(int)type].GetObject();
        }
    }
}
