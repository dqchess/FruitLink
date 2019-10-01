// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

namespace FruitSwipeMatch3Kit
{
    /// <summary>
    /// This class contains the logic associated to the popup that is shown when the player loses a game.
    /// </summary>
    public class LosePopup : EndGamePopup
    {
        private void OnDestroy()
        {
            var playSound = GetComponent<PlaySound>();
            if (playSound != null)
                playSound.Stop("Rain");
        }
    }
}