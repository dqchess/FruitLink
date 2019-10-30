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
    /// This class contains the logic associated to the settings popup that can be
    /// accessed from the home screen.
    /// </summary>
	public class SettingsPopup : Popup
	{
#pragma warning disable 649
        [SerializeField]
        private SpriteSwapper soundSlider;

        [SerializeField]
        private SpriteSwapper musicSlider;

        [SerializeField]
        private AnimatedButton resetProgressButton;

        [SerializeField]
        private Image resetProgressImage;

#pragma warning restore 649

        private int currentSound;
        private int currentMusic;

        protected override void Awake()
        {
            base.Awake();
            Assert.IsNotNull(soundSlider);
            Assert.IsNotNull(musicSlider);
            Assert.IsNotNull(resetProgressButton);
            Assert.IsNotNull(resetProgressImage);
        }

        protected override void Start()
        {
            base.Start();
            musicSlider.SetEnabled(PlayerPrefs.GetInt("music_enabled") == 1);
            soundSlider.SetEnabled(PlayerPrefs.GetInt("sound_enabled") == 1);
        }

        public void OnResetProgressButtonPressed()
        {
            PlayerPrefs.SetInt(GameplayConstants.LastSelectedLevelPrefKey, 0);
            PlayerPrefs.SetInt("next_level", 0);
            for (var i = 1; i <= 30; i++)
            {
                PlayerPrefs.DeleteKey($"level_stars_{i}");
            }

            resetProgressImage.color = Color.gray;
            resetProgressButton.Interactable = false;
            resetProgressImage.GetComponentInChildren<TextMeshProUGUI>().color = Color.gray;
        }

        public void OnSoundSliderValueChanged()
        {
            var homeScreen = ParentScreen as HomeScreen;
            homeScreen.OnSoundButtonPressed();
            homeScreen.UpdateButtons();
        }

        public void OnMusicSliderValueChanged()
        {
            var homeScreen = ParentScreen as HomeScreen;
            homeScreen.OnMusicButtonPressed();
            homeScreen.UpdateButtons();
        }
	}
}
