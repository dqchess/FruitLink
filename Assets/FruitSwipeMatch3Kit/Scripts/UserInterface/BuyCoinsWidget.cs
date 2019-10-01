﻿// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

namespace FruitSwipeMatch3Kit
{
    /// <summary>
    /// This class is used to manage the widget to buy coins that is located in the level screen.
    /// </summary>
    public class BuyCoinsWidget : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField]
        private GameConfiguration gameConfig;
        
        [SerializeField]
        private CoinsSystem coinsSystem;
        
        [SerializeField]
        private TextMeshProUGUI numCoinsText;
#pragma warning restore 649

        private void Awake()
        {
            Assert.IsNotNull(numCoinsText);
        }

        private void Start()
        {
            if (!PlayerPrefs.HasKey("num_coins"))
                PlayerPrefs.SetInt("num_coins", gameConfig.InitialCoins);
            var numCoins = PlayerPrefs.GetInt("num_coins");
            numCoinsText.text = numCoins.ToString("n0");
            coinsSystem.Subscribe(OnCoinsChanged);
        }

        private void OnDestroy()
        {
            coinsSystem.Unsubscribe(OnCoinsChanged);
        }

        public void OnBuyButtonPressed()
        {
            var scene = FindObjectOfType<BaseScreen>();
            var buyCoinsPopup = FindObjectOfType<BuyCoinsPopup>();
            if (scene != null && buyCoinsPopup == null)
                scene.OpenPopup<BuyCoinsPopup>("Popups/BuyCoinsPopup");
        }

        private void OnCoinsChanged(int numCoins)
        {
            numCoinsText.text = numCoins.ToString("n0");
        }
    }
}
