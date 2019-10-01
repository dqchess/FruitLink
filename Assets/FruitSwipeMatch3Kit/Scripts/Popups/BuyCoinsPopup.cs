// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.Assertions;

namespace FruitSwipeMatch3Kit
{
    /// <summary>
    /// This class contains the logic associated to the popup for buying coins.
    /// </summary>
    public class BuyCoinsPopup : Popup
    {
#pragma warning disable 649
        [SerializeField]
        private GameConfiguration gameConfig;

        [SerializeField]
        private CoinsSystem coinsSystem;
        
        [SerializeField]
        private GameObject purchasableItems;

        [SerializeField]
        private GameObject purchasableItemPrefab;
#pragma warning restore 649

        public CoinsSystem CoinsSystem => coinsSystem;

        private PurchasableItem currentPurchasableItem;
        private Popup loadingPopup;

        protected override void Awake()
        {
            base.Awake();
            Assert.IsNotNull(purchasableItems);
            Assert.IsNotNull(purchasableItemPrefab);
        }

        protected override void Start()
        {
            base.Start();
            CoinsSystem.Subscribe(OnCoinsChanged);

            foreach (var item in gameConfig.IapItems)
            {
                var row = Instantiate(purchasableItemPrefab, purchasableItems.transform, false);
                row.GetComponent<PurchasableItem>().Fill(item);
                row.GetComponent<PurchasableItem>().BuyCoinsPopup = this;
            }
        }

        private void OnDestroy()
        {
            coinsSystem.Unsubscribe(OnCoinsChanged);
        }

        public void OnBuyButtonPressed(int numCoins)
        {
            coinsSystem.BuyCoins(numCoins);
        }

        public void OnCloseButtonPressed()
        {
            Close();
        }

        private void OnCoinsChanged(int numCoins)
        {
            if (currentPurchasableItem != null)
                currentPurchasableItem.PlayCoinParticles();
            SoundPlayer.PlaySoundFx("BuyCoinsButton");
        }

        public void OpenLoadingPopup()
        {
            #if UNITY_IOS
            ParentScreen.OpenPopup<LoadingPopup>("Popups/LoadingPopup",
                popup => { loadingPopup = popup; });
            #endif
        }

        public void CloseLoadingPopup()
        {
            #if UNITY_IOS
            if (loadingPopup != null)
            {
                loadingPopup.Close();
            }
            #endif
        }

        public void SetCurrentPurchasableItem(PurchasableItem item)
        {
            currentPurchasableItem = item;
        }
    }
}
