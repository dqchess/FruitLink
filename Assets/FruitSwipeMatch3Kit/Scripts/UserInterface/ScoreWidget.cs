// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store EULA,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FruitSwipeMatch3Kit
{
    /// <summary>
    /// This class is used to manage the widget that displays the current score in the game screen.
    /// </summary>
    public class ScoreWidget : MonoBehaviour, IRestartable
    {
#pragma warning disable 649
        [SerializeField]
        private StarWidget star1Widget;
        [SerializeField]
        private StarWidget star2Widget;
        [SerializeField]
        private StarWidget star3Widget;

        [SerializeField]
        private Image progressBarImage;
        [SerializeField]
        private TextMeshProUGUI scoreText;

        [SerializeField]
        private Animator girlAnimator;
#pragma warning restore 649

        private bool star1Achieved;
        private bool star2Achieved;
        private bool star3Achieved;

        private int star1;
        private int star2;
        private int star3;

        public void Initialize(LevelData levelData)
        {
            star1 = levelData.Star1Score;
            star2 = levelData.Star2Score;
            star3 = levelData.Star3Score;
            UpdateScore(0);
        }

        public void UpdateScore(int score)
        {
            scoreText.text = score.ToString();
            progressBarImage.fillAmount = GetProgressValue(score) / 100.0f;

            if (score >= star1 && !star1Achieved)
            {
                star1Achieved = true;
                star1Widget.SetAsCompleted(true);
                girlAnimator.SetTrigger("Happy");
            }
            if (score >= star2 && !star2Achieved)
            {
                star2Achieved = true;
                star2Widget.SetAsCompleted(true);
                girlAnimator.SetTrigger("Happy");
            }
            if (score >= star3 && !star3Achieved)
            {
                star3Achieved = true;
                star3Widget.SetAsCompleted(true);
                girlAnimator.SetTrigger("Happy");
            }

            var offset = 260.0f;
            var localPosition = progressBarImage.transform.localPosition;
            star1Widget.transform.localPosition = localPosition +
                                                 new Vector3(
                                                     progressBarImage.rectTransform.rect.width *
                                                     (GetProgressValue(star1) / 100.0f) - offset, 0, 0);
            star2Widget.transform.localPosition = localPosition +
                                                 new Vector3(
                                                     progressBarImage.rectTransform.rect.width *
                                                     (GetProgressValue(star2) / 100.0f) - offset, 0, 0);
            star3Widget.transform.localPosition = localPosition +
                                                 new Vector3(progressBarImage.rectTransform.rect.width - offset, 0, 0);
        }

        private int GetProgressValue(int value)
        {
            const int oldMin = 0;
            var oldMax = star3;
            const int newMin = 0;
            const int newMax = 100;
            var oldRange = oldMax - oldMin;
            const int newRange = newMax - newMin;
            var newValue = (((value - oldMin) * newRange) / oldRange) + newMin;
            return newValue;
        }
        
        public void OnGameRestarted()
        {
            star1Achieved = false;
            star2Achieved = false;
            star3Achieved = false;
            star1Widget.OnGameRestarted();
            star2Widget.OnGameRestarted();
            star3Widget.OnGameRestarted();
            UpdateScore(0);
        }
        
    }
}
