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

        public async UniTask InitializeLeaderBoardData()
        {
            // Load existing leaderboard data
            await _manager.LoadLeaderBoardFromJsonAsync();

            // Get the loaded users
            var userList = _scriptable.leaderBoardUsers.ToList();

            // Get current user data
            var currentUser = _gameConfig.currentUserData;

            // Check if user already exists by userId (NOT by reference)
            var existingUserIndex = userList.FindIndex(u =>
                !string.IsNullOrEmpty(u.userId) &&
                !string.IsNullOrEmpty(currentUser.userId) &&
                u.userId == currentUser.userId);

            if (existingUserIndex >= 0)
            {
                // User exists - update their score if new score is higher
                var existingUser = userList[existingUserIndex];
                if (currentUser.score > existingUser.score)
                {
                    existingUser.score = currentUser.score;
                    existingUser.userName = currentUser.userName; // Update name too in case it changed
                    existingUser.date = DateTime.Now.ToString("yyyy-MM-dd");
                    existingUser.time = DateTime.Now.ToString("HH:mm:ss");
                }

                // Mark as current player
                existingUser.isCurrentPlayer = true;
            }
            else
            {
                // New user - add to list
                var newUser = currentUser.CloneViaSerialization();
                newUser.isCurrentPlayer = true;
                newUser.date = DateTime.Now.ToString("yyyy-MM-dd");
                newUser.time = DateTime.Now.ToString("HH:mm:ss");
                userList.Add(newUser);
            }

            // Sort by score (highest first)
            userDataList.Value = userList.OrderByDescending(user => user.score).ToList();

            var userDatas = userDataList.Value;

            // Update ranks and find current player
            for (var i = 0; i < userDatas.Count; i++)
            {
                userDatas[i].rank = i + 1;

                // Check by userId instead of reference comparison
                if (!string.IsNullOrEmpty(userDatas[i].userId) &&
                    !string.IsNullOrEmpty(currentUser.userId) &&
                    userDatas[i].userId == currentUser.userId)
                {
                    userDatas[i].isCurrentPlayer = true;
                    currentPlayerRank.Value = userDatas[i].rank;
                }
                else
                {
                    userDatas[i].isCurrentPlayer = false;
                }
            }

            // Save back to scriptable and file
            _scriptable.leaderBoardUsers = userDatas;
            totalUserCount.Value = userDataList.Value.Count;
            await _manager.SaveLeaderboardToJsonAsync();

            UnityEngine.Debug.Log(
                $"Leaderboard initialized. Total users: {totalUserCount.Value}, Current player rank: {currentPlayerRank.Value}");
        }
    }
}