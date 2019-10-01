// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;

namespace FruitSwipeMatch3Kit
{
    /// <summary>
    /// Helper component to play a sound effect using the sound player. It is
    /// used in all the popups and buttons of the game.
    /// </summary>
    public class PlaySound : MonoBehaviour
    {
        public void Play(string soundName)
        {
            SoundPlayer.PlaySoundFx(soundName);
        }
        
        public void Stop(string soundName)
        {
            SoundPlayer.StopSoundFx(soundName);
        }
    }
}
