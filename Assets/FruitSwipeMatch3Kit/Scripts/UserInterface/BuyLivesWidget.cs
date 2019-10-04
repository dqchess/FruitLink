// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace FruitSwipeMatch3Kit
{
    /// <summary>
    /// This class is used to manage the widget to buy lives that is located in the level screen.
    /// </summary>
    public class BuyLivesWidget : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField]
        private GameConfiguration gameConfig;
        
        [SerializeField]
        private Sprite enabledLifeSprite;

        [SerializeField]
        private Sprite disabledLifeSprite;

        [SerializeField]
        private TextMeshProUGUI numLivesText;

        [SerializeField]
        private TextMeshProUGUI timeToNextLifeText;

        [SerializeField]
        private Image buttonImage;

        [SerializeField]
        private Sprite enabledButtonSprite;

        [SerializeField]
        private Sprite disabledButtonSprite;
#pragma warning restore 649

        private CheckForFreeLives freeLivesChecker;

        private void Awake()
        {
            Assert.IsNotNull(gameConfig);
            Assert.IsNotNull(enabledLifeSprite);
            Assert.IsNotNull(disabledLifeSprite);
            Assert.IsNotNull(numLivesText);
            Assert.IsNotNull(timeToNextLifeText);
            Assert.IsNotNull(buttonImage);
            Assert.IsNotNull(enabledButtonSprite);
            Assert.IsNotNull(disabledButtonSprite);
        }

        private void Start()
        {
            freeLivesChecker = FindObjectOfType<CheckForFreeLives>();
            
            var numLives = PlayerPrefs.GetInt("num_lives");
            var maxLives = gameConfig.MaxLives;
            numLivesText.text = numLives.ToString();
            buttonImage.gameObject.SetActive(numLives < maxLives);
            freeLivesChecker.Subscribe(OnLivesCountdownUpdated, OnLivesCountdownFinished);
        }

        private void OnDestroy()
        {
           freeLivesChecker.Unsubscribe(OnLivesCountdownUpdated, OnLivesCountdownFinished);
        }

        public void OnBuyButtonPressed()
        {
            var numLives = PlayerPrefs.GetInt("num_lives");
            var maxLives = gameConfig.MaxLives;
            if (numLives < maxLives)
            {
                var scene = FindObjectOfType<BaseScreen>();
                var buyLivesPopup = FindObjectOfType<BuyLivesPopup>();
                if (scene != null && buyLivesPopup == null)
                    scene.OpenPopup<BuyLivesPopup>("Popups/BuyLivesPopup");
            }
        }

        private void OnLivesCountdownUpdated(TimeSpan timeSpan, int lives)
        {
            timeToNextLifeText.text = $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
            numLivesText.text = lives.ToString();
//            lifeImage.sprite = lives == 0 ? disabledLifeSprite : enabledLifeSprite;
            var maxLives = gameConfig.MaxLives;
            buttonImage.gameObject.SetActive(lives < maxLives);
        }

        private void OnLivesCountdownFinished(int lives)
        {
            timeToNextLifeText.text = "Full";
            numLivesText.text = lives.ToString();
//            lifeImage.sprite = lives == 0 ? disabledLifeSprite : enabledLifeSprite;
            buttonImage.gameObject.SetActive(false);
        }
    }
}