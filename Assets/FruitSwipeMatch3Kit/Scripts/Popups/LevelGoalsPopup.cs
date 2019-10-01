// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Assertions;

namespace FruitSwipeMatch3Kit
{
    /// <summary>
    /// This class contains the logic associated to the popup that shows the goals
    /// of the level when a game starts.
    /// </summary>
    public class LevelGoalsPopup : Popup
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
        private GameObject goalGroup;

        [SerializeField]
        private GameObject goalPrefab;
#pragma warning restore 649

        protected override void Awake()
        {
            base.Awake();
            Assert.IsNotNull(goalGroup);
            Assert.IsNotNull(goalPrefab);
        }

        protected override void Start()
        {
            base.Start();
            StartCoroutine(AutoKill());
        }

        private IEnumerator AutoKill()
        {
            yield return new WaitForSeconds(2.2f);
            Close();
            /*var gameScreen = ParentScreen as GameScreen;
            if (gameScreen != null)
                gameScreen.GameLogic.StartGame();*/
        }

        public void SetGoals(LevelData levelData)
        {
            var availableColors = new List<ColorTileType>();
            var numColors = PlayerPrefs.GetInt("num_available_colors");
            for (var i = 0; i < numColors; i++)
                availableColors.Add((ColorTileType)PlayerPrefs.GetInt($"available_colors_{i}"));
            
		    PlayerPrefs.DeleteKey("num_available_colors");
		    for (var i = 0; i < numColors; i++)
			    PlayerPrefs.DeleteKey($"available_colors_{i}");
            
            foreach (var goal in levelData.Goals)
            {
                var goalItem = Instantiate(goalPrefab, goalGroup.transform, false);
                
                Sprite sprite = null;
                if (goal.Type == GoalType.CollectTiles)
                    sprite = colorTileSprites[(int)goal.ColorTileType];
                else if (goal.Type == GoalType.CollectRandomTiles)
                    sprite = colorTileSprites[(int)availableColors[(int)goal.RandomColorTileType]];
                else if (goal.Type == GoalType.CollectSlots)
                    sprite = slotSprites[(int)goal.SlotType];
                else if (goal.Type == GoalType.CollectBlockers)
                    sprite = blockerTileSprites[(int)goal.BlockerType];
                else if (goal.Type == GoalType.CollectCollectibles)
                    sprite = collectibleTileSprites[(int)goal.CollectibleType];
                    
                goalItem.GetComponent<GoalItem>().Initialize(sprite, goal.Amount);
            }
        }
    }
}
