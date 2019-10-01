// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.UI;

namespace FruitSwipeMatch3Kit
{
    /// <summary>
    /// A single goal item that is used in the end game popups.
    /// </summary>
    public class EndGameGoalItem : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField]
        private Image tileImage;
		
        [SerializeField]
        private Image tickImage;
        
        [SerializeField]
        private Image crossImage;
#pragma warning restore 649

        public void Initialize(Sprite sprite, bool completed)
        {
            tileImage.sprite = sprite;
            tickImage.enabled = completed;
            crossImage.enabled = !completed;
        }
    }
}
