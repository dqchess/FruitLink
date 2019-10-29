// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using Unity.Entities;
using UnityEngine;
using UnityEngine.Assertions;

namespace FruitSwipeMatch3Kit
{
    /// <summary>
    /// This system is the base system used for all the power-ups in the game.
    /// </summary>
    public abstract class PowerupResolutionSystem : ComponentSystem
    {
        protected EntityQuery query;
        protected EntityArchetype applyGravityArchetype;
        
        protected LevelCreationSystem levelCreationSystem;
        protected PlayerInputSystem inputSystem;

        protected GameScreen gameScreen;
        protected Camera mainCamera;

        protected bool isResolvingPowerup;
        
        protected readonly LayerMask tileLayer = 1 << LayerMask.NameToLayer("Tile");
        protected readonly LayerMask slotLayer = 1 << LayerMask.NameToLayer("Slot");
        protected readonly LayerMask blockerLayer = 1 << LayerMask.NameToLayer("Blocker");
        protected readonly RaycastHit2D[] raycastResults = new RaycastHit2D[8];

        protected override void OnCreate()
        {
            applyGravityArchetype = EntityManager.CreateArchetype(typeof(ApplyGravityData));
            Enabled = false;
        }
       
        public void Initialize()
        {
            levelCreationSystem = World.GetExistingSystem<LevelCreationSystem>();
            inputSystem = World.GetExistingSystem<PlayerInputSystem>();

            gameScreen = Object.FindObjectOfType<GameScreen>();
            Assert.IsNotNull(gameScreen);
            
            mainCamera = Camera.main;
            Assert.IsNotNull(mainCamera);
        }

        protected override void OnUpdate()
        {
            Entities.With(query).ForEach(e =>
            {
                PostUpdateCommands.DestroyEntity(e);
                inputSystem.LockInput();
                isResolvingPowerup = true;
            });

            if (isResolvingPowerup)
                ResolvePowerup();
        }

        protected virtual void ResolvePowerup()
        {
        }

        protected virtual void OnResolvedPowerup()
        {
            var e = EntityManager.CreateEntity(applyGravityArchetype);
            EntityManager.SetComponentData(e, new ApplyGravityData
            {
                MatchSize = 0,
                MatchIndex = 0,
                MatchDirection = MoveDirection.Vertical
            });

            inputSystem.UnlockInput();
            isResolvingPowerup = false;
            inputSystem.DestroySuggetion();
            gameScreen.DisablePowerupOverlay();
        }
    }
}
