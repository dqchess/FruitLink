// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using TMPro;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace FruitSwipeMatch3Kit
{
    /// <summary>
    /// This class is used to manage the button to boy a power-up that is located in the game screen.
    /// </summary>
	public class PowerupButton : MonoBehaviour
	{
#pragma warning disable 649
		[SerializeField]
		private PowerupType powerupType = PowerupType.Crusher;

		[SerializeField]
		private Sprite lockedSprite;

		[SerializeField]
		private Image powerupImage;

		[SerializeField]
		private Sprite moreSprite;

		[SerializeField]
		private Sprite amountSprite;

		[SerializeField]
		private GameObject button;

		[SerializeField]
		private Image buttonImage;

		[SerializeField]
		private TextMeshProUGUI amountText;
#pragma warning restore 649

		private GameScreen gameScreen;

		private bool isAvailable;

		public void Initialize(GameScreen screen, Sprite boosterSprite, int amount, bool available)
		{
			gameScreen = screen;

			isAvailable = available;

			if (isAvailable)
			{
				powerupImage.sprite = boosterSprite;
				powerupImage.SetNativeSize();
				if (amount > 0)
				{
					amountText.text = PlayerPrefs.GetInt($"num_boosters_{(int)powerupType}").ToString();
					buttonImage.sprite = amountSprite;
					amountText.gameObject.SetActive(true);
				}
				else
				{
					buttonImage.sprite = moreSprite;
					amountText.gameObject.SetActive(false);
				}
			}
			else
			{
				powerupImage.sprite = lockedSprite;
				powerupImage.SetNativeSize();
				button.SetActive(false);
			}
		}

		public void OnButtonPressed()
		{
			if (!isAvailable)
				return;

			var amount = PlayerPrefs.GetInt($"num_boosters_{(int)powerupType}");
			if (amount > 0)
			{
				buttonImage.sprite = amountSprite;
				ResolvePowerup(powerupType);
				amount -= 1;
				if (amount == 0)
				{
					buttonImage.sprite = moreSprite;
					amountText.gameObject.SetActive(false);
				}

				PlayerPrefs.SetInt($"num_boosters_{(int) powerupType}", amount);
				amountText.text = amount.ToString();

				gameScreen.EnablePowerupOverlay();
			    switch (powerupType)
			    {
				    case PowerupType.Crusher:
					    gameScreen.SetPowerupText("Select a tile");
					    break;

				    case PowerupType.Bomb:
					    gameScreen.SetPowerupText("Select a tile");
					    break;

				    case PowerupType.Swap:
					    gameScreen.SetPowerupText("Swap two tiles");
					    break;

				    case PowerupType.ColorBomb:
					    gameScreen.SetPowerupText("Select a tile");
					    break;
			    }
			}
			else
			{
				buttonImage.sprite = moreSprite;
				amountText.gameObject.SetActive(false);
				gameScreen.OpenPopup<BuyPowerupsPopup>("Popups/BuyPowerupsPopup",
					popup => { popup.Initialize(powerupType, this); });
			}
		}

		public void UpdateAmount(int newAmount)
		{
			if (newAmount > 0)
			{
				buttonImage.sprite = amountSprite;
				amountText.gameObject.SetActive(true);
				amountText.text = newAmount.ToString();
			}
			else
			{
				buttonImage.sprite = moreSprite;
				amountText.gameObject.SetActive(false);
			}
		}

		private void ResolvePowerup(PowerupType type)
		{
			var world = World.Active;
			var entityMgr = world.EntityManager;

			switch (type)
			{
				case PowerupType.Crusher:
					entityMgr.CreateEntity(typeof(ResolveCrusherPowerupEvent));
					break;

				case PowerupType.Bomb:
					entityMgr.CreateEntity(typeof(ResolveBombPowerupEvent));
					break;

				case PowerupType.Swap:
					entityMgr.CreateEntity(typeof(ResolveSwapPowerupEvent));
					break;

				case PowerupType.ColorBomb:
					entityMgr.CreateEntity(typeof(ResolveColorBombPowerupEvent));
					break;
			}
		}
	}
}
