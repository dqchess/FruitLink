﻿// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store EULA,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FruitSwipeMatch3Kit
{
    /// <summary>
    /// Scriptable object that stores the data of a level.
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "LevelData", menuName = "Fruit Swipe Match 3 Kit/Level", order = 1)]
    public class LevelData : ScriptableObject
    {
        public int Number => int.Parse(name);
        public int Width;
        public int Height;
        public int Moves;
        public int Star1Score;
        public int Star2Score;
        public int Star3Score;
        public bool IsCrusherAvailable;
        public bool IsBombAvailable;
        public bool IsSwapAvailable;
        public bool IsColorBombAvailable;
        public bool EndGameAward = true;
        public List<LevelTileData> Tiles = new List<LevelTileData>();
        public List<LevelGoalData> Goals = new List<LevelGoalData>();
        public List<ColorTileType> AvailableColors = new List<ColorTileType>();

        public void Initialize()
        {
            foreach (var value in Enum.GetValues(typeof(ColorTileType)))
            {
                AvailableColors.Add((ColorTileType)value);
            }
        }

        public void InitGoal()
        {
            foreach (var goal in Goals)
            {
                if (goal.Type == GoalType.CollectSlots)
                {
                    if (goal.Amount == -1)
                    {
                        goal.Amount = Tiles.FindAll(x => x.SlotType == goal.SlotType).Count;
                    }
                }
                else if (goal.Type == GoalType.CollectBlockers)
                {
                    if (goal.Amount == -1)
                    {
                        goal.Amount = Tiles.FindAll(x => x.TileType == TileType.Blocker && x.BlockerType == goal.BlockerType).Count;
                    }
                }
            }
        }
    }
}
