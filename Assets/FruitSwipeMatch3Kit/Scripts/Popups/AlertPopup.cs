// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

namespace FruitSwipeMatch3Kit
{
    /// <summary>
    /// This class contains the logic associated to the alert popup.
    /// </summary>
    public class AlertPopup : Popup
    {
#pragma warning disable 649
        [SerializeField]
        private TextMeshProUGUI textLabel;
#pragma warning restore 649
        public Action OnClose;

        protected override void Awake()
        {
            base.Awake();
            Assert.IsNotNull(textLabel);
        }

        public void OnButtonPressed()
        {
            Close();
        }

        public void OnCloseButtonPressed()
        {
            OnClose?.Invoke();
            Close();
        }

        public void SetText(string text)
        {
            textLabel.text = text;
        }
    }
}