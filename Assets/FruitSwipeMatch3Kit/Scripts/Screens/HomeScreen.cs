// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store EULA,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;

namespace FruitSwipeMatch3Kit
{
    /// <summary>
    /// This class manages the high-level logic of the home screen.
    /// </summary>
    public class HomeScreen : BaseScreen
    {
#pragma warning disable 649
        [SerializeField]
        private GameObject bgMusicPrefab;
		
        [SerializeField]
        private GameObject purchaseManagerPrefab;

        [SerializeField]
        private AnimatedButton musicButton;
        
        [SerializeField]
        private AnimatedButton soundButton;
#pragma warning restore 649
		
        protected override void Start()
        {
            base.Start();
			UpdateButtons();
            var bgMusic = FindObjectOfType<BackgroundMusic>();
            if (bgMusic == null)
                Instantiate(bgMusicPrefab);
			
#if FRUIT_SWIPE_ENABLE_IAP
			var purchaseManager = FindObjectOfType<PurchaseManager>();
			if (purchaseManager == null)
				Instantiate(purchaseManagerPrefab);
#endif
        }
		
        public void OnSettingsButtonPressed()
        {
            OpenPopup<SettingsPopup>("Popups/SettingsPopup");
        }

        public void OnMusicButtonPressed()
        {
			var currentMusic = PlayerPrefs.GetInt("music_enabled");
			currentMusic = 1 - currentMusic;	
            SoundPlayer.SetMusicEnabled(currentMusic == 1);
            PlayerPrefs.SetInt("music_enabled", currentMusic);
        }
        
        public void OnSoundButtonPressed()
        {
			var currentSound = PlayerPrefs.GetInt("sound_enabled");
			currentSound = 1 - currentSound;	
            SoundPlayer.SetSoundEnabled(currentSound == 1);
            PlayerPrefs.SetInt("sound_enabled", currentSound);
        }

        public void UpdateButtons()
        {
	        var music = PlayerPrefs.GetInt("music_enabled", 1);
	        musicButton.GetComponent<SpriteSwapper>().SetEnabled(music == 1);
	        var sound = PlayerPrefs.GetInt("sound_enabled", 1);
	        soundButton.GetComponent<SpriteSwapper>().SetEnabled(sound == 1);
        }
    }
}
