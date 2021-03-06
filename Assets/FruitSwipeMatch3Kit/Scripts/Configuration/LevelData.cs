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
        public bool IsArrowDown = false;
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
                    if(goal.SlotType == SlotType.Ice) 
                        goal.Amount = Tiles.FindAll(x => x.SlotType == SlotType.Ice || 
                                                         x.SlotType == SlotType.Ice2 || 
                                                         x.SlotType == SlotType.Ice3).Count;
                    if(goal.SlotType == SlotType.Jelly)
                        goal.Amount = Tiles.FindAll(x => x.SlotType == SlotType.Jelly || 
                                                         x.SlotType == SlotType.Jelly2 || 
                                                         x.SlotType == SlotType.Jelly3).Count;
                    if(goal.SlotType == SlotType.Vines)
                        goal.Amount = Tiles.FindAll(x => x.SlotType == SlotType.Vines).Count;
                }
                else if (goal.Type == GoalType.CollectBlockers)
                {
                    if(goal.BlockerType == BlockerType.Stone) 
                        goal.Amount = Tiles.FindAll(x => x.BlockerType == BlockerType.Stone ||
                                                         x.BlockerType == BlockerType.Stone2 ||
                                                         x.BlockerType == BlockerType.Stone3).Count;
                    if(goal.BlockerType == BlockerType.Wood) 
                        goal.Amount = Tiles.FindAll(x => x.BlockerType == BlockerType.Wood ||
                                                         x.BlockerType == BlockerType.Wood2 ||
                                                         x.BlockerType == BlockerType.Wood3).Count;
                }
            }
        }
    }
}
