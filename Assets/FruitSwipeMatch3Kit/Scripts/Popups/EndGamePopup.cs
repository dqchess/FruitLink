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
    /// This class contains the logic associated to the end game (win/lose) popup.
    /// </summary>
	public class EndGamePopup : Popup
	{
#pragma warning disable 649
        [SerializeField]
        private TextMeshProUGUI scoreText;

        [SerializeField]
        private GameObject goalGroup;

	    [SerializeField]
	    private GameObject endGameGoalWidgetPrefab;
#pragma warning restore 649
	    
        protected override void Awake()
        {
            base.Awake();
            Assert.IsNotNull(scoreText);
            Assert.IsNotNull(goalGroup);
            Assert.IsNotNull(endGameGoalWidgetPrefab);
        }

	    /*protected override void Start()
	    {
	        base.Start();
	        if (PlayerPrefs.GetInt("sound_enabled") == 1)
    	        GetComponent<AudioSource>().Play();
	    }*/

	    public virtual void OnCloseButtonPressed()
	    {
            var gameScreen = ParentScreen as GameScreen;
            if (gameScreen != null)
            {
                gameScreen.ExitGame();
            }
            else
            {
                GetComponent<ScreenTransition>().PerformTransition();
            }
	    }

        public void OnReplayButtonPressed()
        {
            var gameScreen = ParentScreen as GameScreen;
            if (gameScreen != null)
            {
                var numLives = PlayerPrefs.GetInt("num_lives");
                if (numLives > 0)
                {
                    gameScreen.OnGameRestarted();
                    Close();
                }
                else
                {
                    gameScreen.OpenPopup<BuyLivesPopup>("Popups/BuyLivesPopup");
                }
            }
        }

        public void SetScore(int score)
        {
            scoreText.text = score.ToString();
        }

        public void SetGoals(List<LevelGoalData> goals, GoalsWidget goalsWidget)
        {
            var i = 0;
            foreach (var _ in goals)
            {
                var goalObject = Instantiate(endGameGoalWidgetPrefab, goalGroup.transform, false);
                var widget = goalsWidget.transform.GetChild(i).GetComponent<GoalWidget>();
                goalObject.GetComponent<EndGameGoalItem>().Initialize(
                    widget.transform.GetChild(0).GetComponent<Image>().sprite,
                    widget.IsCompleted());
                ++i;
            }
        }
	}
}
