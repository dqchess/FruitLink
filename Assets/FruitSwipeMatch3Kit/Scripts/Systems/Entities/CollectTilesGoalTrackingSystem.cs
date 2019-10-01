// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store EULA,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;
using Unity.Entities;
using UnityEngine;

namespace FruitSwipeMatch3Kit
{
    /// <summary>
    /// This system is responsible for tracking the state of the
    /// tiles collected by the player.
    /// </summary>
    [UpdateAfter(typeof(CheckWinConditionSystem))]
    [UpdateAfter(typeof(UpdateRemainingMovesUiSystem))]
    [UpdateAfter(typeof(UpdateScoreSystem))]
    public class CollectTilesGoalTrackingSystem : ComponentSystem
    {
        private EntityQuery query;

        private int[] numDestroyedTiles;
        private GoalsWidget goalsWidget;

        protected override void OnCreate()
        {
            Enabled = false;
            
            query = GetEntityQuery(
                ComponentType.ReadOnly<TileDestroyedEvent>());

            numDestroyedTiles = new int[Enum.GetValues(typeof(ColorTileType)).Length];
        }

        public void Initialize()
        {
            goalsWidget = GameObject.Find("GoalsCanvas/GoalsWidget").GetComponent<GoalsWidget>();
        }

        protected override void OnUpdate()
        {
            Entities.With(query).ForEach((Entity entity, ref TileDestroyedEvent evt) =>
            {
                PostUpdateCommands.DestroyEntity(entity);
                numDestroyedTiles[(int)evt.Type] += 1;
                UpdateUi(evt);
            });
        }

        private void Reset()
        {
            for (var i = 0; i < numDestroyedTiles.Length; ++i)
                numDestroyedTiles[i] = 0;
        }

        private void UpdateUi(TileDestroyedEvent evt)
        {
            goalsWidget.OnTileDestroyed(evt);
        }
    }
}
