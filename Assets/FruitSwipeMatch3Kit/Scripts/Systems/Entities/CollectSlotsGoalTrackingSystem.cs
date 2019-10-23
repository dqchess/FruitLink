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
    /// slots collected by the player.
    /// </summary>
    [UpdateAfter(typeof(CheckWinConditionSystem))]
    [UpdateAfter(typeof(UpdateRemainingMovesUiSystem))]
    [UpdateAfter(typeof(UpdateScoreSystem))]
    public class CollectSlotsGoalTrackingSystem : ComponentSystem
    {
        private EntityQuery destroyQuery;
        private EntityQuery instantiateQuery;

        private int[] numDestroyedSlots;
        private GoalsWidget goalsWidget;

        protected override void OnCreate()
        {
            Enabled = false;
            destroyQuery = GetEntityQuery(ComponentType.ReadOnly<SlotDestroyedEvent>());
            instantiateQuery = GetEntityQuery(ComponentType.ReadOnly<SlotInstantiatedEvent>());
            
            numDestroyedSlots = new int[Enum.GetValues(typeof(SlotType)).Length];
        }
        
        public void Initialize()
        {
            goalsWidget = GameplayUI.Instance.GoalsWidget;
        }

        protected override void OnUpdate()
        {
            Entities.With(instantiateQuery).ForEach((Entity entity, ref SlotInstantiatedEvent evt) =>
            {
                PostUpdateCommands.DestroyEntity(entity);
                goalsWidget.OnSlotInstantiated(evt);
            });

            bool isJellyDestroy = false;
            Entities.With(destroyQuery).ForEach((Entity entity, ref SlotDestroyedEvent evt) =>
            {
                PostUpdateCommands.DestroyEntity(entity);
                numDestroyedSlots[(int)evt.Type] += 1;
                goalsWidget.OnSlotDestroyed(evt);
                if (evt.Type == SlotType.Jelly || evt.Type == SlotType.Jelly2 || evt.Type == SlotType.Jelly3)
                    isJellyDestroy = true;
            });
            
            if (isJellyDestroy)
            {
                EntityManager.CreateEntity(typeof(JellyDestroyedEvent));
            }
        }

        private void Reset()
        {
            for (var i = 0; i < numDestroyedSlots.Length; ++i)
                numDestroyedSlots[i] = 0;
        }
    }
}
