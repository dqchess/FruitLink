// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace FruitSwipeMatch3Kit
{
    /// <summary>
    /// This class contains the logic associated to the popup that is shown before
    /// starting a game.
    /// </summary>
    public class StartGamePopup : Popup
    {
#pragma warning disable 649
        [SerializeField]
        private List<Sprite> colorTileSprites;
       
        [SerializeField]
        private List<Sprite> slotSprites;
        
        [SerializeField]
        private List<Sprite> blockerTileSprites;
        
        [SerializeField]
        private List<Sprite> collectibleTileSprites;
        
        [SerializeField]
        private TextMeshProUGUI levelText;
        
        [SerializeField]
        private Sprite enabledStarSprite;

        [SerializeField]
        private Image star1Image;

        [SerializeField]
        private Image star2Image;

        [SerializeField]
        private Image star3Image;

        [SerializeField]
        private GameObject goalPrefab;

        [SerializeField]
        private GameObject goalGroup;

        [SerializeField]
        private GameObject playButton;
#pragma warning restore 649

        private int numLevel;

        protected override void Awake()
        {
            base.Awake();
            Assert.IsNotNull(levelText);
            Assert.IsNotNull(enabledStarSprite);
            Assert.IsNotNull(star1Image);
            Assert.IsNotNull(star2Image);
            Assert.IsNotNull(star3Image);
            Assert.IsNotNull(goalPrefab);
            Assert.IsNotNull(goalGroup);
            Assert.IsNotNull(playButton);
        }

        public void LoadLevelData(int levelNum)
        {
            numLevel = levelNum;

            var level = FileUtils.LoadLevel(numLevel);
            levelText.text = "Level " + numLevel;
            var stars = PlayerPrefs.GetInt("level_stars_" + numLevel);
            if (stars == 1)
            {
                star1Image.sprite = enabledStarSprite;
            }
            else if (stars == 2)
            {
                star1Image.sprite = enabledStarSprite;
                star2Image.sprite = enabledStarSprite;
            }
            else if (stars == 3)
            {
                star1Image.sprite = enabledStarSprite;
                star2Image.sprite = enabledStarSprite;
                star3Image.sprite = enabledStarSprite;
            }

            var randomColors = new List<ColorTileType>();
            randomColors.AddRange(level.AvailableColors);
            randomColors.Shuffle();

            PlayerPrefs.SetInt("num_available_colors", randomColors.Count);
            for (var i = 0; i < randomColors.Count; i++)
            {
                var color = randomColors[i];
                PlayerPrefs.SetInt($"available_colors_{i}", (int)color);
            }

            foreach (var goal in level.Goals)
            {
                var goalItem = Instantiate(goalPrefab, goalGroup.transform, false);
                
                Sprite sprite = null;
                if (goal.Type == GoalType.CollectTiles)
                    sprite = colorTileSprites[(int)goal.ColorTileType];
                else if (goal.Type == GoalType.CollectRandomTiles)
                    sprite = colorTileSprites[(int)randomColors[(int)goal.RandomColorTileType]];
                else if (goal.Type == GoalType.CollectSlots)
                    sprite = slotSprites[(int)goal.SlotType];
                else if (goal.Type == GoalType.CollectBlockers)
                    sprite = blockerTileSprites[(int)goal.BlockerType];
                else if (goal.Type == GoalType.CollectCollectibles)
                    sprite = collectibleTileSprites[(int)goal.CollectibleType];
                    
                goalItem.GetComponent<GoalItem>().Initialize(sprite, goal.Amount);
            }
        }

        public void OnPlayButtonPressed()
        {
            PlayerPrefs.SetInt(GameplayConstants.LastSelectedLevelPrefKey, numLevel);
            GetComponent<ScreenTransition>().PerformTransition();
        }
    }
}
