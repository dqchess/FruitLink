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
        private TutorialData data;
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
                data = Resources.Load<TutorialData>($"Tutorials/{levelNumber}");
                if(data == null) return;
                GameState.IsTutorial = true;
                GameState.TutorialSequence = DOTween.Sequence();
                GameState.TutorialSequence.AppendInterval(GameplayConstants.OpenPopupDelay);
                GameState.TutorialSequence.AppendCallback(() =>
                {
                    if (data.TutorialType == TutorialType.Move)
                    {
                        ShowPath(data.IdxList, data.Hand);
                    }

                    if (data.TutorialType == TutorialType.Crusher)
                    {
                        if (PlayerPrefs.GetInt(GameplayConstants.CrusherTutorialPrefKey) > 0)
                        {
                            GameState.IsTutorial = false;
                            return;
                        }
                        PlayerPrefs.SetInt(GameplayConstants.CrusherTutorialPrefKey, 1);
                        Object.FindObjectOfType<GameScreen>().OpenPopup<PowerTutorialPopup>("Popups/CrusherTutorialPopup", popup =>
                        {
                            popup.OnClose += () => ShowPath(data.IdxList, data.Hand);
                        });
                    }
                    if (data.TutorialType == TutorialType.Bomb)
                    {
                        if (PlayerPrefs.GetInt(GameplayConstants.BombTutorialPrefKey) > 0)
                        {
                            GameState.IsTutorial = false;
                            return;
                        }
                        PlayerPrefs.SetInt(GameplayConstants.BombTutorialPrefKey, 1);
                        Object.FindObjectOfType<GameScreen>().OpenPopup<PowerTutorialPopup>("Popups/BombTutorialPopup", popup =>
                        {
                            popup.OnClose += () => ShowPath(data.IdxList, data.Hand);
                        });
                    }
                    if (data.TutorialType == TutorialType.Swap)
                    {
                        if (PlayerPrefs.GetInt(GameplayConstants.SwapTutorialPrefKey) > 0)
                        {
                            GameState.IsTutorial = false;
                            return;
                        }
                        PlayerPrefs.SetInt(GameplayConstants.SwapTutorialPrefKey, 1);
                        Object.FindObjectOfType<GameScreen>().OpenPopup<PowerTutorialPopup>("Popups/SwapTutorialPopup", popup =>
                        {
                            popup.OnClose += () => ShowPath(data.IdxList, data.Hand);
                        });
                    }
                    if (data.TutorialType == TutorialType.ColorBomb)
                    {
                        if (PlayerPrefs.GetInt(GameplayConstants.ColorBombTutorialPrefKey) > 0)
                        {
                            GameState.IsTutorial = false;
                            return;
                        }
                        PlayerPrefs.SetInt(GameplayConstants.ColorBombTutorialPrefKey, 1);
                        Object.FindObjectOfType<GameScreen>().OpenPopup<PowerTutorialPopup>("Popups/ColorBombTutorialPopup", popup =>
                        {
                            popup.OnClose += () => ShowPath(data.IdxList, data.Hand);
                        });
                    } 
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

        public void ClearDarkTile()
        {
            PlayerInputSystem playerInput = World.GetExistingSystem<PlayerInputSystem>();
            playerInput.RemoveDarkFruits();
        }

        public void ShowPath(int step)
        {
            if (step == 2)
            {
                Destroy();
                ShowPath(data.IdxStep2);
            }
        }

        private void ShowPath(List<int> idxList, GameObject handPrefab = null)
        {
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

            if (handPrefab != null)
            {
                hand = Object.Instantiate(handPrefab, vector3[0], Quaternion.identity).transform;
                var t = hand.DOPath(vector3, idxList.Count / 2f);
                t.SetLoops(-1);   
            }
        }
    }
}