// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;
using TMPro;
using UnityEngine;

namespace FruitSwipeMatch3Kit
{
    /// <summary>
    /// This class contains the logic associated to the generic confirmation popup.
    /// </summary>
    public class ConfirmationPopup : Popup
    {
#pragma warning disable 649
        [SerializeField]
        private TextMeshProUGUI primaryText;
		
        [SerializeField]
        private TextMeshProUGUI secondaryText;
#pragma warning restore 649
		
        private Action onAcceptAction;
		
        public void OnAcceptButtonPressed()
        {
            onAcceptAction();
        }

        public void SetInfo(string primary, string secondary, Action onAccept)
        {
            primaryText.text = primary;
            secondaryText.text = secondary;
            onAcceptAction = onAccept;
        }
    }
}