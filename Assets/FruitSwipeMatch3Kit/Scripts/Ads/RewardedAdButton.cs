// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;

#if UNITY_ADS
using UnityEngine.Advertisements;
#endif

namespace FruitSwipeMatch3Kit
{
    /// <summary>
    /// The rewarded advertisement button that is present in the level screen.
    /// </summary>
    public class RewardedAdButton : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField] private GameConfiguration gameConfig;

        [SerializeField] private CoinsSystem coinsSystem;

        [SerializeField] private LevelScreen levelScreen;
#pragma warning restore 649

        public void ShowRewardedAd()
        {
#if !UNITY_EDITOR
	        bool isRewarded = false;
	        Admob.Instance.ShowReward(() => isRewarded = true, () =>
	        {
		        if (isRewarded)
		        {
			        var rewardCoins = gameConfig.RewardedAdCoins;
			        coinsSystem.BuyCoins(rewardCoins);
			        levelScreen.OpenPopup<AlertPopup>("Popups/AlertPopup", popup =>
			        {
				        popup.SetText($"You earned {rewardCoins} coins!");
			        });
					gameObject.SetActive(false);
		        }
	        });
#endif
        }
    }
}