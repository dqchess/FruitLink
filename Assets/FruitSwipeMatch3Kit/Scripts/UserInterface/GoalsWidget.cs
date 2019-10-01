// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store EULA,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace FruitSwipeMatch3Kit
{
    /// <summary>
    /// This class is used to manage the widget to display the level goals that is located in the game screen.
    /// </summary>
    public class GoalsWidget : MonoBehaviour, IRestartable
    {
#pragma warning disable 649
        [SerializeField]
        private GameScreen gameScreen;
        
        [SerializeField]
        private GameObject goalPrefab;
#pragma warning restore 649

        private readonly List<GoalWidget> widgets = new List<GoalWidget>(4);

        private void Awake()
        {
            Assert.IsNotNull(gameScreen);
            Assert.IsNotNull(goalPrefab);
        }

        public void Initialize(List<LevelGoalData> goals, TilePools tilePools)
        {
            foreach (var goal in goals)
            {
                var obj = Instantiate(goalPrefab, transform, false);
                var widget = obj.GetComponent<GoalWidget>();
                widget.Initialize(goal, tilePools);
                AddWidget(widget);
            }
        }

        private void AddWidget(GoalWidget widget)
        {
            widgets.Add(widget);
        }

        public void OnTileDestroyed(TileDestroyedEvent evt)
        {
            foreach (var widget in widgets)
                widget.OnTileDestroyed(evt);
        }

        public void OnSlotDestroyed(SlotDestroyedEvent evt)
        {
            foreach (var widget in widgets)
                widget.OnSlotDestroyed(evt);
        }
        
        public void OnBlockerDestroyed(BlockerDestroyedEvent evt)
        {
            foreach (var widget in widgets)
                widget.OnBlockerDestroyed(evt);
        }
        
        public void OnCollectibleCollected(CollectibleCollectedEvent evt)
        {
            foreach (var widget in widgets)
                widget.OnCollectibleCollected(evt);
        }

        public bool HasPlayerWon()
        {
            var playerWon = true;
            
            foreach (var widget in widgets)
            {
                if (!widget.IsCompleted())
                {
                    playerWon = false;
                    break;
                }
            }

            return playerWon;
        }

        public void OnGameRestarted()
        {
            foreach (var widget in widgets)
                widget.OnGameRestarted();
        }
    }
}
