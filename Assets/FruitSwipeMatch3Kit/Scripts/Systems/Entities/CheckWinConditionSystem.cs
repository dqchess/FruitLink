// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store EULA,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using Unity.Entities;
using UnityEngine;

namespace FruitSwipeMatch3Kit
{
    /// <summary>
    /// This system checks if the player has won or lost the game after
    /// the gravity has been applied to the level.
    /// </summary>
    [UpdateAfter(typeof(UpdateRemainingMovesUiSystem))]
    public class CheckWinConditionSystem : ComponentSystem
    {
        private EntityQuery query;
        private EntityArchetype matchEndArchetype;
        private GameScreen gameScreen;

        protected override void OnCreate()
        {
            Enabled = false;
            query = GetEntityQuery(ComponentType.ReadOnly<GravityFinishedEvent>());
            matchEndArchetype = EntityManager.CreateArchetype(typeof(MatchEndEvent));
        }

        public void Initialize()
        {
            gameScreen = Object.FindObjectOfType<GameScreen>();
        }

        protected override void OnUpdate()
        {
            var playingEndGameSequence = false;
            Entities.With(query).ForEach(entity =>
            {
                PostUpdateCommands.DestroyEntity(entity);
                
                if (GameState.IsPlayingEndGameSequence)
                {
                    playingEndGameSequence = true;
                }
                else
                {
                    var widgets = Object.FindObjectOfType<GoalsWidget>();
                    if (widgets.HasPlayerWon())
                    {
                        gameScreen.OnPlayerWon();
                    }
                    else if (GameState.SwapCount >= GameplayConstants.GameOverSwapCount)
                    {
                        gameScreen.OpenLosePopup();
                    }
                    else
                    {
                        var updateMovesSystem = World.GetExistingSystem<UpdateRemainingMovesUiSystem>();
                        if (updateMovesSystem.NumRemainingMoves == 0)
                            gameScreen.OnPlayerLost();
                    }
                }
            });

            if (playingEndGameSequence)
                gameScreen.AdvanceEndGameSequence();
            else
                EntityManager.CreateEntity(matchEndArchetype);
        }
    }
}
