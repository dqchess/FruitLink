// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FruitSwipeMatch3Kit
{
    /// <summary>
    /// A single goal item.
    /// </summary>
    public class GoalItem : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField]
        private Image tileImage;
		
        [SerializeField]
        private TextMeshProUGUI amountText;
#pragma warning restore 649

        public void Initialize(Sprite sprite, int amount)
        {
            tileImage.sprite = sprite;
            amountText.text = amount.ToString();
        }
    }
}