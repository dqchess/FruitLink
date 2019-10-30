﻿// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Purchasing;
using UnityEngine.UI;

namespace FruitSwipeMatch3Kit
{
    /// <summary>
    /// An in-app purchasable item in the shop popup.
    /// </summary>
    public class PurchasableItem : MonoBehaviour
    {
        [HideInInspector]
        public BuyCoinsPopup BuyCoinsPopup;

#pragma warning disable 649
        [SerializeField]
        private GameObject mostPopular;
        [SerializeField]
        private GameObject bestValue;
        [SerializeField]
        private GameObject discount;
        [SerializeField]
        private TextMeshProUGUI discountText;
        [SerializeField]
        private TextMeshProUGUI numCoinsText;
        [SerializeField]
        private TextMeshProUGUI priceText;
        [SerializeField]
        private Image coinsImage;
        [SerializeField]
        private List<Sprite> coinIcons;
//        [SerializeField]
//        private ParticleSystem coinsParticles;
#pragma warning restore 649
        private IStoreController storeController;
        private IapItem cachedItem;

        private void Awake()
        {
            Assert.IsNotNull(mostPopular);
            Assert.IsNotNull(bestValue);
            Assert.IsNotNull(discount);
            Assert.IsNotNull(discountText);
            Assert.IsNotNull(numCoinsText);
            Assert.IsNotNull(priceText);
            Assert.IsNotNull(coinsImage);
//            Assert.IsNotNull(coinsParticles);
        }

        public void Fill(PurchaseManager purchaseManager, IapItem item)
        {
            cachedItem = item;
            storeController = purchaseManager.Controller;
            numCoinsText.text = item.NumCoins.ToString("n0");
            if (item.Discount > 0)
                discountText.text = $"{item.Discount}%";
            else
                discount.SetActive(false);

            if (item.MostPopular)
            {
                bestValue.SetActive(false);
            }
            else if (item.BestValue)
            {
                mostPopular.SetActive(false);
            }
            else
            {
                mostPopular.SetActive(false);
                bestValue.SetActive(false);
            }

            coinsImage.sprite = coinIcons[(int)item.CoinIcon];
            coinsImage.SetNativeSize();
            
            if (storeController != null)
            {
                var product = storeController.products.WithID(item.StoreId);
                if (product != null)
                    priceText.text = product.metadata.localizedPriceString;
            }
            else
            {
                priceText.text = item.DefaultPrices;
            }
        }

        public void OnPurchaseButtonPressed()
        {
            if (storeController != null)
            {
                storeController.InitiatePurchase(cachedItem.StoreId);
                BuyCoinsPopup.SetCurrentPurchasableItem(this);
                BuyCoinsPopup.OpenLoadingPopup();
            }
        }

//        public void PlayCoinParticles()
//        {
//            coinsParticles.Play();
//        }
    }
}
