// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store EULA,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.UI;

namespace FruitSwipeMatch3Kit
{
    /// <summary>
    /// This class is used to manage the star that is displayed in the game screen's score widget.
    /// </summary>
    public class StarWidget : MonoBehaviour, IRestartable
    {
#pragma warning disable 649
        [SerializeField]
        private Sprite completedSprite;
        [SerializeField]
        private Sprite notCompletedSprite;
        [SerializeField]
        private ParticleSystem particles;
#pragma warning restore 649

        private Image starImage;
        private Animator animator;
        
        private static readonly int Achieved = Animator.StringToHash("Achieved");

        private void Awake()
        {
            starImage = GetComponent<Image>();
            animator = GetComponent<Animator>();
        }
        
        public void SetAsCompleted(bool completed)
        {
            starImage.sprite = completed ? completedSprite : notCompletedSprite;
            if (completed)
            {
                animator.SetTrigger(Achieved);
                particles.Play();
                SoundPlayer.PlaySoundFx("StarProgressBar");
            }
        }

        public void OnGameRestarted()
        {
            SetAsCompleted(false);
        }
    }
}
