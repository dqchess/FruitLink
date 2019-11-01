// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

namespace FruitSwipeMatch3Kit
{
    /// <summary>
    /// This class contains the logic associated to the popup that is displayed
    /// to the user when he runs out of moves during a game.
    /// </summary>
	public class OutOfMovesPopup : Popup
	{
#pragma warning disable 649
		[SerializeField]
		private GameConfiguration gameConfig;
		
		[SerializeField]
		private CoinsSystem coinsSystem;
		
		[SerializeField]
		private TextMeshProUGUI numMovesText;
		
		[SerializeField]
		private TextMeshProUGUI questionText;
		
		[SerializeField]
		private TextMeshProUGUI priceText;
#pragma warning restore 649

		private GameScreen gameScreen;

		protected override void Awake()
		{
			base.Awake();
			Assert.IsNotNull(numMovesText);
			Assert.IsNotNull(questionText);
			Assert.IsNotNull(priceText);
		}

		protected override void Start()
		{
			base.Start();
			gameScreen = ParentScreen as GameScreen;
			numMovesText.text = $"+ {gameConfig.NumExtraMoves}";
			questionText.text = $"Add {gameConfig.NumExtraMoves} moves to continue?";
			priceText.text = gameConfig.ExtraMovesCost.ToString();
		}
		
		public void OnQuitButtonPressed()
		{
			Close();
			gameScreen.CloseTopCanvas();
			gameScreen.OpenLosePopup();
		}
		
		public void OnBuyButtonPressed()
		{
			var numCoins = PlayerPrefs.GetInt("num_coins");
			var cost = gameConfig.ExtraMovesCost;
			if (numCoins >= cost)
			{
				coinsSystem.SpendCoins(cost);
				//coinParticles.Play();
				//SoundManager.instance.PlaySound("CoinsPopButton");
				Close();
				gameScreen.ContinueGame();
			}
			else
			{
				//SoundManager.instance.PlaySound("Button");
				gameScreen.OpenCoinsPopup();
			}
		}
	}
}
