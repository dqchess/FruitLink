// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store EULA,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

namespace FruitSwipeMatch3Kit
{
    public class LevelGoal : IRestartable
    {
        public bool IsCompleted => amount == 0;
        public LevelGoalData Data => data;
        public GoalType Type => data.Type;
        public int Amount => amount;

        private readonly LevelGoalData data;
        private int amount;

        public LevelGoal(LevelGoalData goalData)
        {
            data = goalData;
            amount = data.Amount;
        }
        
        public void UpdateAmount(int diff)
        {
            amount += diff;
            if (amount < 0)
                amount = 0;
        }

        public void OnGameRestarted()
        {
            amount = 0;
        }
    }
}
