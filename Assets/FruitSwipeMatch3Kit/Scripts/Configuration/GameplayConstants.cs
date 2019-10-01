// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store EULA,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

namespace FruitSwipeMatch3Kit
{
    /// <summary>
    /// General gameplay-related settings.
    /// </summary>
    public static class GameplayConstants
    {
        public const int NumTilesNeededForMatch = 3;
            
        public const float FallingExistingTilesSpeed = 0.3f;

        public const float GravityAfterBoosterDelay = 0.5f;

        public const float WinPopupDelay = 0.5f;
        public const float LosePopupDelay = 0.5f;
        public const float OutOfMovesPopupDelay = 0.5f;
        public const float EndGameAwardPopupDelay = 0.5f;

        public const float EndGameSequenceSpawnFrequency = 1.0f;
        public const float EndGameSequenceExplosionFrequency = 2.0f;

        public const string LastSelectedLevelPrefKey = "last_selected_level";
        public const string NextLevelPrefKey = "next_level";
        public const string UnlockedNextLevelPrefKey = "unlocked_next_level";
    }
}
