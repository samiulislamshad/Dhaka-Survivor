using System;
using System.Collections.Generic;
using Systems.InputSystem.Model;
using UniRx;

namespace Systems.LeaderBoardSystem.Model
{
    [Serializable]
    public class LeaderBoardModel
    {
        public ReactiveProperty<List<UserData>> userDataList = new();
        public ReactiveProperty<int> totalUserCount = new();
        public ReactiveProperty<float> elementHeight = new(100f);
    
        public void InitializeWithTestData()
        {
            var testData = new List<UserData>();
            for (var i = 1; i <= 500; i++)
            {
                testData.Add(new UserData(
                    rank: i,
                    userName: $"Player_{i}",
                    score: (10000 - i * 10).ToString(),
                    userId: $"user_{i}"
                ));
            }
        
            userDataList.Value = testData;
            totalUserCount.Value = testData.Count;
        }
    }
}