// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store EULA,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using Unity.Entities;
using UnityEngine;

namespace FruitSwipeMatch3Kit
{
    /// <summary>
    /// This system is responsible for updating the current score
    /// of a game.
    /// </summary>
    public class UpdateScoreSystem : ComponentSystem, IRestartable
    {
        private ScoreWidget scoreWidget;

        private GameConfiguration gameConfig;
        private GameState gameState;
        private int score;

        protected override void OnCreate()
        {
            Enabled = false;
        }

        public void Initialize()
        {
            score = 0;

            scoreWidget = GameplayUI.Instance.ScoreWidget;
            scoreWidget.UpdateScore(score);

            var gameScreen = Object.FindObjectOfType<GameScreen>();
            gameState = gameScreen.GameState;
            gameConfig = gameScreen.GameConfig;
        }

        protected override void OnUpdate()
        {
            var scoreChange = 0;
            Entities.WithAll<TileDestroyedEvent>().ForEach(entity =>
            {
                scoreChange += gameConfig.DefaultTileScore;
            });

            if (scoreChange > 0)
            {
                score += scoreChange;
                scoreWidget.UpdateScore(score);
                gameState.Score = score;
            }
        }

        public void OnGameRestarted()
        {
            score = 0;
            gameState.OnGameRestarted();
            scoreWidget.OnGameRestarted();
        }
    }
}
