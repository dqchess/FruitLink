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

        [SerializeField] private Color[] colors;
        [SerializeField] private GameObject selectionLine;
        [SerializeField] private GameObject selectionParticle;
        [SerializeField] private ObjectPool suggetionPool;
#pragma warning restore 649
        public GameObject SelectionLine => selectionLine;
        public GameObject SelectionParticle => selectionParticle;
        public ObjectPool SuggetionPool => suggetionPool;

        public Color GetColorTile(ColorTileType color)
        {
            return colors[(int) color];
        }
        
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
            
            suggetionPool.Initialize();
        }

        public GameObject GetColorTileParticles(ColorTileType type)
        {
            return colorTilePools[(int)type].GetObject();
        }
        
        public GameObject GetSlotParticles(SlotType type)
        {
            int typeIndex = 0;
            switch (type)
            {
                case SlotType.Normal:
                    typeIndex = 0;
                    break;
                case SlotType.Ice:
                    typeIndex = 1;
                    break;
                case SlotType.Ice2:
                    typeIndex = 1;
                    break;
                case SlotType.Ice3:
                    typeIndex = 1;
                    break;
                case SlotType.Jelly:
                    typeIndex = 2;
                    break;
                case SlotType.Jelly2:
                    typeIndex = 2;
                    break;
                case SlotType.Jelly3:
                    typeIndex = 2;
                    break;
            }
            return slotPools[typeIndex].GetObject();
        }
        
        public GameObject GetBlockerParticles(BlockerType type)
        {            
            int typeIndex = 0;
            switch (type)
            {
                case BlockerType.Block:
                    typeIndex = 0;
                    break;
                case BlockerType.Stone:
                    typeIndex = 1;
                    break;
                case BlockerType.Stone2:
                    typeIndex = 1;
                    break;
                case BlockerType.Stone3:
                    typeIndex = 1;
                    break;
                case BlockerType.Wood:
                    typeIndex = 2;
                    break;
                case BlockerType.Wood2:
                    typeIndex = 2;
                    break;
                case BlockerType.Wood3:
                    typeIndex = 2;
                    break;
            }
            return blockerPools[typeIndex].GetObject();
        }
        
        public GameObject GetCollectibleParticles(CollectibleType type)
        {
            return collectiblePools[(int)type].GetObject();
        }
    }
}
