// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using DG.Tweening;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace FruitSwipeMatch3Kit
{
    /// <summary>
    /// This class manages the high-level logic of the level screen.
    /// </summary>
    public class LevelScreen : BaseScreen
    {
#pragma warning disable 649
        [SerializeField]
        private ScrollRect scrollRect;

        [SerializeField]
        private GameObject scrollView;

        [SerializeField]
        private GameObject avatarPrefab;

        [SerializeField]
        private GameObject rewardedAdButton;
#pragma warning restore 649

        private void Awake()
        {
            Assert.IsNotNull(scrollRect);
            Assert.IsNotNull(scrollView);
            Assert.IsNotNull(avatarPrefab);
            Assert.IsNotNull(rewardedAdButton);
        }

        protected override void Start()
        {
            base.Start();
            
            scrollRect.vertical = false;

            var progressionSystem = World.Active.GetExistingSystem<GameProgressionSystem>();
            progressionSystem.IsPlayerComingFromLevelScreen = true;

            #if !UNITY_EDITOR
            rewardedAdButton.SetActive(Admob.Instance.IsRewardLoaded());
            #endif

            var avatar = Instantiate(avatarPrefab, scrollView.transform.GetChild(0), false);

            var nextLevel = PlayerPrefs.GetInt("next_level");
            if (nextLevel == 0)
                nextLevel = 1;

            LevelButton currentButton = null;
            foreach (var button in scrollView.GetComponentsInChildren<LevelButton>())
            {
                if (button.NumLevel != nextLevel)
                    continue;
                currentButton = button;
                break;
            }

            if (currentButton == null)
                return;

            var newPos = scrollView.GetComponent<RectTransform>().anchoredPosition;
            newPos.y =
                scrollRect.transform.InverseTransformPoint(scrollView.GetComponent<RectTransform>().position).y -
                scrollRect.transform.InverseTransformPoint(currentButton.transform.position).y;
            newPos.y += Canvas.GetComponent<RectTransform>().rect.height / 2.0f;
            if (newPos.y < scrollView.GetComponent<RectTransform>().anchoredPosition.y)
                scrollView.GetComponent<RectTransform>().anchoredPosition = newPos;

            var targetPos = currentButton.transform.position + new Vector3(0, 1.8f, 0);

            LevelButton prevButton = null;
            if (PlayerPrefs.GetInt(GameplayConstants.UnlockedNextLevelPrefKey) == 1)
            {
                foreach (var button in scrollView.GetComponentsInChildren<LevelButton>())
                {
                    if (button.NumLevel != PlayerPrefs.GetInt(GameplayConstants.LastSelectedLevelPrefKey))
                        continue;
                    prevButton = button;
                    break;
                }
            }

            if (prevButton != null)
            {
                avatar.transform.position = prevButton.transform.position + new Vector3(0, 1.8f, 0);
                var sequence = DOTween.Sequence();
                sequence.AppendInterval(0.5f);
                sequence.Append(avatar.transform.DOMove(targetPos, 0.8f));
                sequence.AppendCallback(() => avatar.GetComponent<LevelAvatar>().StartFloatingAnimation());
                sequence.AppendCallback(() => scrollRect.vertical = true);
            }
            else
            {
                avatar.transform.position = targetPos;
                avatar.GetComponent<LevelAvatar>().StartFloatingAnimation();
                scrollRect.vertical = true;
            }
        }
    }
}
