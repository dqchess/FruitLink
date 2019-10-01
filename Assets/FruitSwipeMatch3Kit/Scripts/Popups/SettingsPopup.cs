// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

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
        private Slider soundSlider;

        [SerializeField]
        private Slider musicSlider;

        [SerializeField]
        private AnimatedButton resetProgressButton;

        [SerializeField]
        private Image resetProgressImage;

        [SerializeField]
        private Sprite resetProgressDisabledSprite;
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
            Assert.IsNotNull(resetProgressDisabledSprite);
        }

        protected override void Start()
        {
            base.Start();
            soundSlider.value = PlayerPrefs.GetInt("sound_enabled");
            musicSlider.value = PlayerPrefs.GetInt("music_enabled");
        }

        public void OnResetProgressButtonPressed()
        {
            PlayerPrefs.SetInt(GameplayConstants.LastSelectedLevelPrefKey, 0);
            PlayerPrefs.SetInt("next_level", 0);
            for (var i = 1; i <= 30; i++)
            {
                PlayerPrefs.DeleteKey($"level_stars_{i}");
            }
            resetProgressImage.sprite = resetProgressDisabledSprite;
            resetProgressButton.Interactable = false;
        }

        public void OnSoundSliderValueChanged()
        {
            currentSound = (int)soundSlider.value;
            SoundPlayer.SetSoundEnabled(currentSound == 1);
            PlayerPrefs.SetInt("sound_enabled", currentSound);
            var homeScreen = ParentScreen as HomeScreen;
            homeScreen.UpdateButtons();
        }

        public void OnMusicSliderValueChanged()
        {
            currentMusic = (int)musicSlider.value;
            SoundPlayer.SetMusicEnabled(currentMusic == 1);
            PlayerPrefs.SetInt("music_enabled", currentMusic);
            var homeScreen = ParentScreen as HomeScreen;
            homeScreen.UpdateButtons();
        }
	}
}
