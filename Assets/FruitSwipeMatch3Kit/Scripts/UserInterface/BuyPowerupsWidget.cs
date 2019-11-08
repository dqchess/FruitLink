// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;
using UnityEngine;

namespace FruitSwipeMatch3Kit
{
    /// <summary>
    /// This class is used to manage the widget to buy power-ups that is located in the game screen.
    /// </summary>
    public class BuyPowerupsWidget : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField]
        private GameScreen gameScreen;
		
        [SerializeField]
        private List<Sprite> boosterSprites;
			
        [SerializeField]
        private List<PowerupButton> buttons;
#pragma warning restore 649

        public void Initialize(LevelData levelData)
        {
            buttons[0].Initialize(gameScreen, boosterSprites[0], PlayerPrefs.GetInt("num_boosters_0", 5), levelData.IsCrusherAvailable);
            buttons[1].Initialize(gameScreen, boosterSprites[1], PlayerPrefs.GetInt("num_boosters_1", 5), levelData.IsBombAvailable);
            buttons[2].Initialize(gameScreen, boosterSprites[2], PlayerPrefs.GetInt("num_boosters_2", 5), levelData.IsSwapAvailable);
            buttons[3].Initialize(gameScreen, boosterSprites[3], PlayerPrefs.GetInt("num_boosters_3", 5), levelData.IsColorBombAvailable);
        }

        public void PressButton(int index)
        {
            buttons[index].OnButtonPressed();
        }
    }
}