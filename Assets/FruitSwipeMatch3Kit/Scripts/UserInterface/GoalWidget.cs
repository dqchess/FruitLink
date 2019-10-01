// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store EULA,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FruitSwipeMatch3Kit
{
    /// <summary>
    /// This class is used to manage the goal that is displayed in the game screen's goals widget.
    /// </summary>
    public class GoalWidget : MonoBehaviour, IRestartable
    {
#pragma warning disable 649
        [SerializeField]
        private List<Sprite> colorTileSprites;
       
        [SerializeField]
        private List<Sprite> slotSprites;
        
        [SerializeField]
        private List<Sprite> blockerTileSprites;
        
        [SerializeField]
        private List<Sprite> collectibleTileSprites;

        [SerializeField]
        private Image image;

        [SerializeField]
        private GameObject amountGroup;
        
        [SerializeField]
        private TextMeshProUGUI label;
       
        [SerializeField]
        private Image tickImage;
        
        [SerializeField]
        private Animator tickAnimator;
#pragma warning restore 649

        private LevelGoal goal;
        private ColorTileType colorTileType;

        private LevelGoalData cachedGoalData;
        private TilePools cachedTilePools;

        private static readonly int Pop = Animator.StringToHash("Pop");

        public void Initialize(LevelGoalData levelGoal, TilePools tilePools)
        {
            cachedGoalData = levelGoal;
            cachedTilePools = tilePools;
            
            goal = new LevelGoal(levelGoal);
            
            amountGroup.SetActive(true);

            Sprite sprite = null;
            if (goal.Type == GoalType.CollectTiles)
                sprite = colorTileSprites[(int)goal.Data.ColorTileType];
            else if (goal.Type == GoalType.CollectRandomTiles)
            {
                var randomPool = tilePools.RandomizedColorTilePools[(int)goal.Data.RandomColorTileType];
                var tileType = randomPool.Prefab.GetComponent<TileDataProxy>().Value.Type;
                sprite = colorTileSprites[(int)tileType];
                colorTileType = tileType;
            }
            else if (goal.Type == GoalType.CollectSlots)
                sprite = slotSprites[(int)goal.Data.SlotType];
            else if (goal.Type == GoalType.CollectBlockers)
                sprite = blockerTileSprites[(int) goal.Data.BlockerType];
            else if (goal.Type == GoalType.CollectCollectibles)
                sprite = collectibleTileSprites[(int) goal.Data.CollectibleType];
            image.sprite = sprite;
            
            SetAmountText(goal.Amount);

            tickImage.enabled = false;
        }

        public void OnTileDestroyed(TileDestroyedEvent evt)
        {
            if (goal.Type == GoalType.CollectTiles &&
                goal.Data.ColorTileType == evt.Type ||
                goal.Type == GoalType.CollectRandomTiles &&
                colorTileType == evt.Type)
                UpdateAmount();
        }

        public void OnSlotDestroyed(SlotDestroyedEvent evt)
        {
            if (goal.Type == GoalType.CollectSlots &&
                goal.Data.SlotType == evt.Type)
                UpdateAmount();
        }
        
        public void OnBlockerDestroyed(BlockerDestroyedEvent evt)
        {
            if (goal.Type == GoalType.CollectBlockers &&
                goal.Data.BlockerType == evt.Type)
                UpdateAmount();
        }
        
        public void OnCollectibleCollected(CollectibleCollectedEvent evt)
        {
            if (goal.Type == GoalType.CollectCollectibles &&
                goal.Data.CollectibleType == evt.Type)
                UpdateAmount();
        }

        public bool IsCompleted()
        {
            return goal.IsCompleted;
        }

        private void UpdateAmount()
        {
            goal.UpdateAmount(-1);

            SetAmountText(goal.Amount);

            if (goal.Amount == 0 && !tickImage.enabled)
            {
                amountGroup.SetActive(false);
                tickImage.enabled = true;
                SoundPlayer.PlaySoundFx("ReachedGoal");
                tickAnimator.SetTrigger(Pop);
            }
        }

        private void SetAmountText(int amount)
        {
            label.text = amount.ToString();
        }

        public void OnGameRestarted()
        {
            Initialize(cachedGoalData, cachedTilePools);
        }
    }
}
