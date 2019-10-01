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
    /// This class manages the level buttons that are displayed on the level scene.
    /// </summary>
    public class LevelButton : MonoBehaviour
    {
        public int NumLevel;

#pragma warning disable 649
        [SerializeField]
        private Sprite currentButtonSprite;

        [SerializeField]
        private Sprite playedButtonSprite;

        [SerializeField]
        private Sprite lockedButtonSprite;

        [SerializeField]
        private Sprite yellowStarSprite;

        [SerializeField]
        private Image buttonImage;

        [SerializeField]
        private TextMeshProUGUI numLevelText;
        
        [SerializeField]
        private TextMeshProUGUI numLevelTextBlue;

        [SerializeField]
        private TextMeshProUGUI numLevelTextPink;

        [SerializeField]
        private GameObject star1;

        [SerializeField]
        private GameObject star2;

        [SerializeField]
        private GameObject star3;
#pragma warning restore 649

        /// <summary>
        /// Unity's Awake method.
        /// </summary>
        private void Awake()
        {
            Assert.IsNotNull(currentButtonSprite);
            Assert.IsNotNull(playedButtonSprite);
            Assert.IsNotNull(lockedButtonSprite);
            Assert.IsNotNull(yellowStarSprite);
            Assert.IsNotNull(buttonImage);
            Assert.IsNotNull(numLevelText);
            Assert.IsNotNull(numLevelTextBlue);
            Assert.IsNotNull(numLevelTextPink);
            Assert.IsNotNull(star1);
            Assert.IsNotNull(star2);
            Assert.IsNotNull(star3);
        }

        /// <summary>
        /// Unity's Start method.
        /// </summary>
        private void Start()
        {
            var numLevelStr = NumLevel.ToString();
            numLevelText.text = numLevelStr;
            numLevelTextBlue.text = numLevelStr;
            numLevelTextPink.text = numLevelStr;
            var nextLevel = PlayerPrefs.GetInt("next_level");
            if (nextLevel == 0)
                nextLevel = 1;

            if (NumLevel == nextLevel)
            {
                buttonImage.sprite = currentButtonSprite;
                star1.SetActive(false);
                star2.SetActive(false);
                star3.SetActive(false);
                numLevelTextBlue.gameObject.SetActive(false);
            }
            else if (NumLevel < nextLevel)
            {
                buttonImage.sprite = playedButtonSprite;
                numLevelTextPink.gameObject.SetActive(false);
                var stars = PlayerPrefs.GetInt("level_stars_" + NumLevel);
                switch (stars)
                {
                    case 1:
                        star1.GetComponent<Image>().sprite = yellowStarSprite;
                        break;

                    case 2:
                        star1.GetComponent<Image>().sprite = yellowStarSprite;
                        star2.GetComponent<Image>().sprite = yellowStarSprite;
                        break;

                    default:
                        star1.GetComponent<Image>().sprite = yellowStarSprite;
                        star2.GetComponent<Image>().sprite = yellowStarSprite;
                        star3.GetComponent<Image>().sprite = yellowStarSprite;
                        break;
                }
            }
            else
            {
                transform.GetComponentInChildren<AnimatedButton>().Interactable = false;
                buttonImage.sprite = lockedButtonSprite;
                numLevelText.gameObject.SetActive(false);
                numLevelTextBlue.gameObject.SetActive(false);
                numLevelTextPink.gameObject.SetActive(false);
                star1.SetActive(false);
                star2.SetActive(false);
                star3.SetActive(false);
            }
        }

        /// <summary>
        /// Called when the button is pressed.
        /// </summary>
        public void OnButtonPressed()
        {
            if (buttonImage.sprite == lockedButtonSprite)
                return;

            var screen = GameObject.Find("LevelScreen").GetComponent<LevelScreen>();
            if (screen != null)
            {
                var numLives = PlayerPrefs.GetInt("num_lives");
                if (numLives > 0)
                {
                    if (!FileUtils.FileExists("Levels/" + NumLevel))
                    {
                        screen.OpenPopup<AlertPopup>("Popups/AlertPopup",
                            popup => popup.SetText("This level does not exist."));
                    }
                    else
                    {
                        screen.OpenPopup<StartGamePopup>("Popups/StartGamePopup", popup =>
                        {
                            popup.LoadLevelData(NumLevel);
                        });
                    }
                }
                else
                {
                    screen.OpenPopup<BuyLivesPopup>("Popups/BuyLivesPopup");
                }
            }
        }
    }
}
