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
    /// collectibles collected by the player.
    /// </summary>
    [UpdateAfter(typeof(CheckWinConditionSystem))]
    [UpdateAfter(typeof(UpdateRemainingMovesUiSystem))]
    [UpdateAfter(typeof(UpdateScoreSystem))]
    public class CollectCollectiblesGoalTrackingSystem : ComponentSystem
    {
        private EntityQuery query;

        private int[] numCollectedCollectibles;
        private GoalsWidget goalsWidget;

        protected override void OnCreate()
        {
            Enabled = false;
            
            query = GetEntityQuery(
                ComponentType.ReadOnly<CollectibleCollectedEvent>());

            numCollectedCollectibles = new int[Enum.GetValues(typeof(CollectibleType)).Length];
        }

        public void Initialize()
        {
            goalsWidget = GameObject.Find("GoalsCanvas/GoalsWidget").GetComponent<GoalsWidget>();
        }

        protected override void OnUpdate()
        {
            Entities.With(query).ForEach((Entity entity, ref CollectibleCollectedEvent evt) =>
            {
                PostUpdateCommands.DestroyEntity(entity);
                numCollectedCollectibles[(int)evt.Type] += 1;
                UpdateUi(evt);
            });
        }

        private void Reset()
        {
            for (var i = 0; i < numCollectedCollectibles.Length; ++i)
                numCollectedCollectibles[i] = 0;
        }

        private void UpdateUi(CollectibleCollectedEvent evt)
        {
            goalsWidget.OnCollectibleCollected(evt);
        }
    }
}
