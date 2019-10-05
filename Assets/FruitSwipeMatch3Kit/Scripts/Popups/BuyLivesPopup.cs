// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace FruitSwipeMatch3Kit
{
    /// <summary>
    /// This class contains the logic associated to the popup for buying lives.
    /// </summary>
    public class BuyLivesPopup : Popup
    {
#pragma warning disable 649
        [SerializeField]
        private GameConfiguration gameConfig;

        [SerializeField]
        private TextMeshProUGUI refillCostText;

        [SerializeField]
        private AnimatedButton refillButton;

        [SerializeField]
        private Image refillButtonImage;

        [SerializeField]
        private Sprite refillButtonDisabledSprite;
#pragma warning restore 649

        protected override void Awake()
        {
            base.Awake();
            Assert.IsNotNull(refillCostText);
            Assert.IsNotNull(refillButton);
            Assert.IsNotNull(refillButtonImage);
            Assert.IsNotNull(refillButtonDisabledSprite);
        }

        protected override void Start()
        {
            base.Start();
            var maxLives = gameConfig.MaxLives;
            var numLives = PlayerPrefs.GetInt("num_lives");
            if (numLives >= maxLives)
                DisableRefillButton();
            refillCostText.text = gameConfig.LivesRefillCost.ToString();
        }

        public void OnRefillButtonPressed()
        {
            var numCoins = PlayerPrefs.GetInt("num_coins");
            if (numCoins >= gameConfig.LivesRefillCost)
            {
                var freeLivesChecker = FindObjectOfType<CheckForFreeLives>();
                if (freeLivesChecker != null)
                {
                    freeLivesChecker.RefillLives();
                    DisableRefillButton();
                }
            }
            else
            {
                var screen = ParentScreen;
                if (screen != null)
                {
                    screen.CloseCurrentPopup();
                    screen.OpenPopup<BuyCoinsPopup>("Popups/BuyCoinsPopup",
                        popup =>
                        {
                            popup.OnClose.AddListener(
                                () =>
                                {
                                    screen.OpenPopup<BuyLivesPopup>("Popups/BuyLivesPopup");
                                });
                        });
                }
            }
        }

        private void DisableRefillButton()
        {
            refillButtonImage.sprite = refillButtonDisabledSprite;
            refillButton.Interactable = false;
        }
    }
}