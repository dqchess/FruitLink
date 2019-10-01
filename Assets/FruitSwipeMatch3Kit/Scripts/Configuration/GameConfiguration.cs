// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FruitSwipeMatch3Kit
{
    /// <summary>
    /// The game configuration type. It stores the general settings of the game.
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "GameConfiguration", menuName = "Fruit Swipe Match 3 Kit/Game configuration", order = 0)]
    public class GameConfiguration : ScriptableObject
    {
        public int DefaultTileScore;
		
        public int MaxLives;
        public int TimeToNextLife;
        public int LivesRefillCost;

        public int InitialCoins;
        
        public int NumTilesNeededForRegularBooster;
        public int NumTilesNeededForCrossBooster;
        public int NumTilesNeededForStarBooster;

        public int CrusherPowerupAmount;
        public int CrusherPowerupPrice;
        public int BombPowerupAmount;
        public int BombPowerupPrice;
        public int SwapPowerupAmount;
        public int SwapPowerupPrice;
        public int ColorBombPowerupAmount;
        public int ColorBombPowerupPrice;

        public int NumExtraMoves;
        public int ExtraMovesCost;

        public float DefaultZoomLevel;
        public float DefaultCanvasScalingMatch;
        public List<ResolutionOverride> ResolutionOverrides = new List<ResolutionOverride>();
        
        public int RewardedAdCoins;
        public List<IapItem> IapItems;
        
        public float GetZoomLevel()
        {
            var zoomLevel = DefaultZoomLevel;
            foreach (var resolution in ResolutionOverrides)
            {
                if (resolution.Width == Screen.width && resolution.Height == Screen.height)
                {
                    zoomLevel = resolution.ZoomLevel;
                    break;
                }
            }
            return zoomLevel;
        }
    }
}