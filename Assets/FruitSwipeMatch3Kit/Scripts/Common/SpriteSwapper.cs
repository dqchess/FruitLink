// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.UI;

namespace FruitSwipeMatch3Kit
{
    /// <summary>
    /// Utility class for swapping the sprite of a UI Image between two
    /// predefined ones representing enabled/disabled states.
    /// </summary>
    public class SpriteSwapper : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField]
        private Sprite enabledSprite;

        [SerializeField]
        private Sprite disabledSprite;
#pragma warning restore 649

        private Image image;

        public void Awake()
        {
            image = GetComponent<Image>();
        }

        public void SwapSprite()
        {
            image.sprite = image.sprite == enabledSprite ? disabledSprite : enabledSprite;
        }

        public void SetEnabled(bool spriteEnabled)
        {
            image.sprite = spriteEnabled ? enabledSprite : disabledSprite;
        }
    }
}