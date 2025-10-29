using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Systems.GameSystem.Config;
using Systems.InputSystem.Model;
using Systems.LeaderBoardSystem.Manager;
using Systems.LeaderBoardSystem.Scriptable;
using UniRx;
using Unity.VisualScripting;

namespace Systems.LeaderBoardSystem.Model
{
    [Serializable]
    public class LeaderBoardModel
    {
        private readonly LeaderboardManager _manager;
        private LeaderBoardScriptable _scriptable;
        private GameConfig _gameConfig;
        
        public ReactiveProperty<List<UserData>> userDataList = new();
        public ReactiveProperty<int> totalUserCount = new();
        public ReactiveProperty<float> elementHeight = new(100f);
        public ReactiveProperty<int> currentPlayerRank = new(-1);

        public LeaderBoardModel(LeaderboardManager manager, LeaderBoardScriptable scriptable, GameConfig gameConfig)
        {
            _manager = manager;
            _scriptable = scriptable;
            _gameConfig = gameConfig;
        }
    
        public UserData GetCurrentPlayerData()
        {
            if (userDataList.Value == null) return null;
            return userDataList.Value.Find(user => user.isCurrentPlayer);
        }

        public void InitializeLeaderBoardData()
        {
            _manager.LoadLeaderboardFromJson();
            var userList = _scriptable.leaderBoardUsers.ToList();
            var currentUser = _gameConfig.currentUserData.CloneViaSerialization();
            userList.Add(currentUser);
            
            userDataList.Value = userList.OrderByDescending(user => user.score).ToList();

            var userDatas = userDataList.Value;
            for (var i = 0; i < userDatas.Count; i++)
            {
                userDatas[i].rank = i+1;
                if (userDatas[i] != currentUser)
                {
                    userDatas[i].isCurrentPlayer = false;
                    continue;
                }
                userDatas[i].isCurrentPlayer = true;
                currentPlayerRank.Value = userDatas[i].rank;
            }
            
            _scriptable.leaderBoardUsers = userDatas;
            _manager.SaveLeaderboardToJsonAsync().Forget();
            
            totalUserCount.Value = userDataList.Value.Count;
            // currentPlayerRank.Value = userDataList.Value.IndexOf(currentUser);
        }

        #region Sorting

        public void SortByRank()
        {
            _scriptable.leaderBoardUsers.Sort((a, b) => a.rank.CompareTo(b.rank));
        }

        // Sort by score and update ranks
        public void SortByScore()
        {
            // Sort by score descending (highest first)
            _scriptable.leaderBoardUsers = _scriptable.leaderBoardUsers
                .OrderByDescending(user => user.score)
                .ToList();

            // Update ranks based on sorted order
            for (int i = 0; i < _scriptable.leaderBoardUsers.Count; i++)
            {
                _scriptable.leaderBoardUsers[i].rank = i + 1;
            }
        }

        // Sort by multiple criteria (score, then time)
        public void SortByScoreAndTime()
        {
            _scriptable.leaderBoardUsers = _scriptable.leaderBoardUsers
                .OrderByDescending(user => user.score)
                .ThenBy(user => user.date)
                .ThenBy(user => user.time)
                .ToList();

            // Update ranks
            for (int i = 0; i < _scriptable.leaderBoardUsers.Count; i++)
            {
                _scriptable.leaderBoardUsers[i].rank = i + 1;
            }
        }

        #endregion
    }
}