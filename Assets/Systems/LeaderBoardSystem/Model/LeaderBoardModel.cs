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
        private readonly PlayFabLeaderboardManager _manager;
        private GameConfig _gameConfig;

        public ReactiveProperty<List<UserData>> userDataList = new();
        public ReactiveProperty<int> totalUserCount = new();
        public ReactiveProperty<float> elementHeight = new(100f);
        public ReactiveProperty<int> currentPlayerRank = new(-1);

        public LeaderBoardModel(PlayFabLeaderboardManager manager, LeaderBoardScriptable scriptable, GameConfig gameConfig)
        {
            _manager = manager;
            _gameConfig = gameConfig;
        }

        public UserData GetCurrentPlayerData()
        {
            if (userDataList.Value == null) return null;
            return userDataList.Value.Find(user => user.isCurrentPlayer);
        }

        public async UniTask AddPlayerDataToLeaderboard()
        {
            var playerData = GetCurrentPlayerData();
            await _manager.AddNewPlayerToLeaderboard(playerData.userName, playerData.score);
        }

        public async UniTask InitializeLeaderBoardData()
        {
            // 1. Fetch the entire leaderboard from PlayFab
            var onlineLeaderboard = await _manager.FetchLeaderboard();
            
            var mergedUsers = new Dictionary<string, UserData>();
            
            // Add online users first
            foreach (var user in onlineLeaderboard)
            {
                if (!string.IsNullOrEmpty(user.userId))
                {
                    mergedUsers[user.userId] = user;
                }
            }

            // 4. Add current player (temporarily, not committed to PlayFab yet)
            var currentUser = _gameConfig.currentUserData;
            if (!string.IsNullOrEmpty(currentUser.userId))
            {
                if (mergedUsers.TryGetValue(currentUser.userId, out var existing))
                {
                    existing.score = currentUser.score;
                    existing.userName = currentUser.userName;

                    existing.isCurrentPlayer = true;
                }
                else
                {
                    // Add new player
                    var newUser = currentUser.CloneViaSerialization();
                    newUser.isCurrentPlayer = true;
                    newUser.date = DateTime.Now.ToString("yyyy-MM-dd");
                    newUser.time = DateTime.Now.ToString("HH:mm:ss");
                    mergedUsers[currentUser.userId] = newUser;
                }
            }
            
            // 5. Sort and assign ranks
            userDataList.Value = mergedUsers.Values.OrderByDescending(u => u.score).ToList();
    
            for (int i = 0; i < userDataList.Value.Count; i++)
            {
                userDataList.Value[i].rank = i + 1;
        
                if (!string.IsNullOrEmpty(userDataList.Value[i].userId) &&
                    userDataList.Value[i].userId == currentUser.userId)
                {
                    currentPlayerRank.Value = userDataList.Value[i].rank;
                }
            }
    
            totalUserCount.Value = userDataList.Value.Count;
        }
    }
}