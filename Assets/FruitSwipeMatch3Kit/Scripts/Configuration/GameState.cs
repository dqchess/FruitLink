// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store EULA,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

namespace FruitSwipeMatch3Kit
{
    /// <summary>
    /// Stores the current state of a game.
    /// </summary>
    public class GameState : IRestartable
    {
        public int Score;

        public void OnGameRestarted()
        {
            Score = 0;
        }
    }
}
