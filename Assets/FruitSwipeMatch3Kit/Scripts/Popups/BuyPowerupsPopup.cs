// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;
using TMPro;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace FruitSwipeMatch3Kit
{
    /// <summary>
    /// This class contains the logic associated to the popup for buying power-ups.
    /// </summary>
	public class BuyPowerupsPopup : Popup
	{
#pragma warning disable 649
		[SerializeField]
		private GameConfiguration gameConfig;

		[SerializeField]
		private CoinsSystem coinsSystem;

		[SerializeField]
		private List<Sprite> powerupSprites;

		[SerializeField]
		private Image powerupImage;

		[SerializeField]
		private TextMeshProUGUI powerupNameText;

		[SerializeField]
		private TextMeshProUGUI powerupAmountText;

		[SerializeField]
		private TextMeshProUGUI powerupDescriptionText;

		[SerializeField]
		private TextMeshProUGUI powerupPriceText;
#pragma warning restore 649

		private PowerupType powerupType;
		private PowerupButton powerupButton;

		protected override void Awake()
		{
			base.Awake();
			Assert.IsNotNull(gameConfig);
			Assert.IsNotNull(coinsSystem);
			Assert.IsNotNull(powerupImage);
			Assert.IsNotNull(powerupNameText);
			Assert.IsNotNull(powerupAmountText);
			Assert.IsNotNull(powerupDescriptionText);
			Assert.IsNotNull(powerupPriceText);

			OnOpen.AddListener(() =>
			{
				var gameScreen = ParentScreen as GameScreen;
				if (gameScreen != null)
				{
					gameScreen.OpenTopCanvas();
					var world = World.Active;
					var inputSystem = world.GetExistingSystem<PlayerInputSystem>();
					inputSystem.LockInput();
				}
			});
			OnClose.AddListener(() =>
			{
				var gameScreen = ParentScreen as GameScreen;
				if (gameScreen != null)
				{
					gameScreen.CloseTopCanvas();
					var world = World.Active;
					var inputSystem = world.GetExistingSystem<PlayerInputSystem>();
					inputSystem.UnlockInput();
				}
			});
		}

		public void Initialize(PowerupType type, PowerupButton button)
		{
			powerupType = type;
			powerupButton = button;
			powerupImage.sprite = powerupSprites[(int)powerupType];
			powerupImage.SetNativeSize();

			switch (powerupType)
			{
				case PowerupType.Crusher:
					powerupNameText.text = "Crusher";
					powerupDescriptionText.text = "Destroys a single tile.";
					powerupAmountText.text = $"x{gameConfig.CrusherPowerupAmount}";
					powerupPriceText.text = gameConfig.CrusherPowerupPrice.ToString();
					break;

				case PowerupType.Bomb:
					powerupNameText.text = "Bomb";
					powerupDescriptionText.text = "Destroys nine tiles.";
					powerupAmountText.text = $"x{gameConfig.BombPowerupAmount}";
					powerupPriceText.text = gameConfig.BombPowerupPrice.ToString();
					break;

				case PowerupType.Swap:
					powerupNameText.text = "Swap";
					powerupDescriptionText.text = "Swaps two tiles.";
					powerupAmountText.text = $"x{gameConfig.SwapPowerupAmount}";
					powerupPriceText.text = gameConfig.SwapPowerupPrice.ToString();
					break;

				case PowerupType.ColorBomb:
					powerupNameText.text = "Color bomb";
					powerupDescriptionText.text = "Destroys all tiles of the selected color.";
					powerupAmountText.text = $"x{gameConfig.ColorBombPowerupAmount}";
					powerupPriceText.text = gameConfig.ColorBombPowerupPrice.ToString();
					break;
			}
		}

		public void OnBuyButtonPressed()
		{
		    var playerPrefsKey = $"num_boosters_{(int)powerupType}";
		    var numBoosters = PlayerPrefs.GetInt(playerPrefsKey);

		    Close();

		    var gameScreen = ParentScreen as GameScreen;
		    if (gameScreen != null)
		    {
			    var cost = GetPowerupCost(powerupType);
				if (!PlayerPrefs.HasKey("num_coins"))
				    PlayerPrefs.SetInt("num_coins", gameConfig.InitialCoins);
			    var coins = PlayerPrefs.GetInt("num_coins");
			    if (cost > coins)
			    {
				    var button = powerupButton;
				    gameScreen.OpenPopup<BuyCoinsPopup>("Popups/BuyCoinsPopup",
					    popup =>
					    {
						    popup.OnClose.AddListener(
							    () =>
							    {
								    gameScreen.OpenPopup<BuyPowerupsPopup>("Popups/BuyPowerupsPopup",
									    buyBoostersPopup => { buyBoostersPopup.Initialize(powerupType, button); });
							    });
					    });
			    }
			    else
			    {
				    coinsSystem.SpendCoins(cost);
                    SoundPlayer.PlaySoundFx("BuyCoinsButton");
				    numBoosters += GetPowerupAmount(powerupType);
				    PlayerPrefs.SetInt(playerPrefsKey, numBoosters);
				    powerupButton.UpdateAmount(numBoosters);
			    }
		    }
		}

		private int GetPowerupAmount(PowerupType type)
		{
			switch (type)
			{
				case PowerupType.Crusher:
					return gameConfig.CrusherPowerupAmount;

				case PowerupType.Bomb:
					return gameConfig.BombPowerupAmount;

				case PowerupType.Swap:
					return gameConfig.SwapPowerupAmount;

				default:
					return gameConfig.ColorBombPowerupAmount;
			}
		}

		private int GetPowerupCost(PowerupType type)
		{
			switch (type)
			{
				case PowerupType.Crusher:
					return gameConfig.CrusherPowerupPrice;

				case PowerupType.Bomb:
					return gameConfig.BombPowerupPrice;

				case PowerupType.Swap:
					return gameConfig.SwapPowerupPrice;

				default:
					return gameConfig.ColorBombPowerupPrice;
			}
		}
	}
}
