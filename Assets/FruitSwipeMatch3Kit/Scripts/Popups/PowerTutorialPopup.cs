using System;
using FruitSwipeMatch3Kit;
using UnityEngine;

public class PowerTutorialPopup : Popup
{
    [SerializeField] private PowerupButton button;
    [SerializeField] private string amountString;
    [SerializeField] private int buttonIndex;
    public Action OnClose;

    protected override void Start()
    {
        base.Start();
        button.Initialize(FindObjectOfType<GameScreen>(), null, PlayerPrefs.GetInt(amountString), true);
    }

    public void OnButtonPressed()
    {
        OnClose?.Invoke();
        FindObjectOfType<BuyPowerupsWidget>().PressButton(buttonIndex);
        Close();
    }
}
