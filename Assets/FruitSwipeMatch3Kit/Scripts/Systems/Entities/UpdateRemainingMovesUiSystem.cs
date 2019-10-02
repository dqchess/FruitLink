// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store EULA,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using TMPro;
using Unity.Entities;
using UnityEngine;

namespace FruitSwipeMatch3Kit
{
    /// <summary>
    /// This system is responsible for updating the number of moves
    /// left in a game on the screen.
    /// </summary>
    public class UpdateRemainingMovesUiSystem : ComponentSystem
    {
        private TextMeshProUGUI label;
        private int numRemainingMoves;

        public int NumRemainingMoves => numRemainingMoves;

        protected override void OnCreate()
        {
            Enabled = false;
        }

        protected override void OnUpdate()
        {
            var matchHappened = false;
            Entities.WithAll<MatchHappenedEvent>().ForEach(entity =>
            {
                PostUpdateCommands.DestroyEntity(entity);
                matchHappened = true;
            });

            if (matchHappened)
            {
                numRemainingMoves -= 1;
                if (numRemainingMoves == 0)
                    numRemainingMoves = 0;
                label.text = numRemainingMoves.ToString();
            }
        }

        public void Initialize(int moves)
        {
            numRemainingMoves = moves;
            label = GameplayUI.Instance.MoveLeftText;
            label.text = numRemainingMoves.ToString();
        }
    }
}
