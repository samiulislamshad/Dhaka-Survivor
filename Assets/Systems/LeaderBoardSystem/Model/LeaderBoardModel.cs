using System;
using System.Collections.Generic;
using Systems.InputSystem.Model;
using UniRx;

namespace Systems.LeaderBoardSystem.Model
{
    [Serializable]
    public class LeaderBoardModel
    {
        public ReactiveProperty<List<UserData>> userDataList = new ReactiveProperty<List<UserData>>();
        public ReactiveProperty<int> totalUserCount = new ReactiveProperty<int>();
        public ReactiveProperty<float> elementHeight = new ReactiveProperty<float>(100f);
        public ReactiveProperty<int> currentPlayerRank = new ReactiveProperty<int>(-1);
    
        public void InitializeWithTestData()
        {
            var testData = new List<UserData>();
            int currentPlayerRank = 250; // Example: current player is at rank 250
        
            for (int i = 1; i <= 500; i++)
            {
                testData.Add(new UserData(
                    rank: i,
                    userName: $"Player_{i}",
                    score: (10000 - i * 10).ToString(),
                    userId: $"user_{i}",
                    isCurrentPlayer: (i == currentPlayerRank) // Mark current player
                ));
            }
        
            userDataList.Value = testData;
            totalUserCount.Value = testData.Count;
            this.currentPlayerRank.Value = currentPlayerRank;
        }
    
        public UserData GetCurrentPlayerData()
        {
            if (userDataList.Value == null) return null;
            return userDataList.Value.Find(user => user.isCurrentPlayer);
        }    }
}