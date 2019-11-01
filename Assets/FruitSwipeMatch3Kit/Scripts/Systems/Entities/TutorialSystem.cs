using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Plugins.Core.PathCore;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace FruitSwipeMatch3Kit
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(LevelCreationSystem))]
    public class TutorialSystem : ComponentSystem
    {
        private Transform hand;
        
        protected override void OnCreate()
        {
            Enabled = false;
        }

        protected override void OnUpdate()
        {
            Entities.ForEach((Entity entity, ref CreateLevelEvent evt) =>
            {
                var levelNumber = evt.Number;
                GameState.IsTutorial = true;
                GameState.TutorialSequence = DOTween.Sequence();
                GameState.TutorialSequence.AppendInterval(GameplayConstants.OpenPopupDelay);
                GameState.TutorialSequence.AppendCallback(() =>
                {
                    TutorialData data = Resources.Load<TutorialData>($"Tutorials/{levelNumber}");
                    List<int> idxList = data.IdxList;
                    LevelCreationSystem levelCreation = World.GetExistingSystem<LevelCreationSystem>();
                    List<float3> tilePositions = levelCreation.TilePositions;
                    Vector3[] vector3 = new Vector3[idxList.Count];
                    for (int i = 0; i < vector3.Length; i++)
                    {
                        float3 pos = tilePositions[idxList[i]];
                        vector3[i] = new Vector3(pos.x, pos.y);
                    }

                    PlayerInputSystem playerInput = World.GetExistingSystem<PlayerInputSystem>();
                    Entity firstEntity = levelCreation.TileEntities[idxList[0]];
                    ColorTileType colorTileType = EntityManager.GetComponentData<TileData>(firstEntity).Type;
                    playerInput.CreatePathHighlight(firstEntity, colorTileType);
                    playerInput.DisplaySuggetion(idxList);

                    hand = Object.Instantiate(data.Hand, vector3[0], Quaternion.identity).transform;
                    var t = hand.DOPath(vector3, idxList.Count / 2f);
                    t.SetLoops(-1);
                });
            });
        }

        public void Destroy()
        {
            if (hand != null)
            {
                hand.DOKill();
                Object.Destroy(hand.gameObject);                
            }
        }
    }
}