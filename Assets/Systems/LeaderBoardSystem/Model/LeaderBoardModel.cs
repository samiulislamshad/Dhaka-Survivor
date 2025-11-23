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
using UnityEngine;

namespace Systems.LeaderBoardSystem.Model
{
    [Serializable]
    public class LeaderBoardModel
    {
        // private readonly LeaderboardManager _manager;
        private readonly PlayFabLeaderboardManager _manager;
        private LeaderBoardScriptable _scriptable;
        private GameConfig _gameConfig;

        public ReactiveProperty<List<UserData>> userDataList = new();
        public ReactiveProperty<int> totalUserCount = new();
        public ReactiveProperty<float> elementHeight = new(100f);
        public ReactiveProperty<int> currentPlayerRank = new(-1);

        public LeaderBoardModel(PlayFabLeaderboardManager manager, LeaderBoardScriptable scriptable, GameConfig gameConfig)
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

        // public async UniTask InitializeLeaderBoardData()
        // {
        //     // Load existing leaderboard data
        //     await _manager.LoadLeaderBoardFromJsonAsync();
        //
        //     // Get the loaded users
        //     var userList = _scriptable.leaderBoardUsers.ToList();
        //
        //     // Get current user data
        //     var currentUser = _gameConfig.currentUserData;
        //
        //     // Check if user already exists by userId (NOT by reference)
        //     var existingUserIndex = userList.FindIndex(u =>
        //         !string.IsNullOrEmpty(u.userId) &&
        //         !string.IsNullOrEmpty(currentUser.userId) &&
        //         u.userId == currentUser.userId);
        //
        //     if (existingUserIndex >= 0)
        //     {
        //         // User exists - update their score if new score is higher
        //         var existingUser = userList[existingUserIndex];
        //         if (currentUser.score > existingUser.score)
        //         {
        //             existingUser.score = currentUser.score;
        //             existingUser.userName = currentUser.userName; // Update name too in case it changed
        //             existingUser.date = DateTime.Now.ToString("yyyy-MM-dd");
        //             existingUser.time = DateTime.Now.ToString("HH:mm:ss");
        //         }
        //
        //         // Mark as current player
        //         existingUser.isCurrentPlayer = true;
        //     }
        //     else
        //     {
        //         // New user - add to list
        //         var newUser = currentUser.CloneViaSerialization();
        //         newUser.isCurrentPlayer = true;
        //         newUser.date = DateTime.Now.ToString("yyyy-MM-dd");
        //         newUser.time = DateTime.Now.ToString("HH:mm:ss");
        //         userList.Add(newUser);
        //     }
        //
        //     // Sort by score (highest first)
        //     userDataList.Value = userList.OrderByDescending(user => user.score).ToList();
        //
        //     var userDatas = userDataList.Value;
        //
        //     // Update ranks and find current player
        //     for (var i = 0; i < userDatas.Count; i++)
        //     {
        //         userDatas[i].rank = i + 1;
        //
        //         // Check by userId instead of reference comparison
        //         if (!string.IsNullOrEmpty(userDatas[i].userId) &&
        //             !string.IsNullOrEmpty(currentUser.userId) &&
        //             userDatas[i].userId == currentUser.userId)
        //         {
        //             userDatas[i].isCurrentPlayer = true;
        //             currentPlayerRank.Value = userDatas[i].rank;
        //         }
        //         else
        //         {
        //             userDatas[i].isCurrentPlayer = false;
        //         }
        //     }
        //
        //     // Save back to scriptable and file
        //     _scriptable.leaderBoardUsers = userDatas;
        //     totalUserCount.Value = userDataList.Value.Count;
        //     await _manager.SaveLeaderboardToJsonAsync();
        //
        //     UnityEngine.Debug.Log(
        //         $"Leaderboard initialized. Total users: {totalUserCount.Value}, Current player rank: {currentPlayerRank.Value}");
        // }

        public async UniTask AddPlayerDataToLeaderboard()
        {
            var playerData = GetCurrentPlayerData();
            await _manager.AddNewPlayerToLeaderboard(playerData.userName, playerData.score);
        }

        public async UniTask InitializeLeaderBoardData()
        {
            // 1. Fetch the entire leaderboard from PlayFab
            var onlineLeaderboard = await _manager.FetchLeaderboard(100);
            
            var mergedUsers = new Dictionary<string, UserData>();
            
            // Add online users first
            foreach (var user in onlineLeaderboard)
            {
                if (!string.IsNullOrEmpty(user.userId))
                {
                    mergedUsers[user.userId] = user;
                    Debug.LogWarning($"user {user.userId}, score: {user.score}");
                }
            }
            //
            // // Add local users if not already in online
            // foreach (var user in localUsers)
            // {
            //     if (!string.IsNullOrEmpty(user.userId) && !mergedUsers.ContainsKey(user.userId))
            //     {
            //         mergedUsers[user.userId] = user;
            //     }
            // }

            // 4. Add current player (temporarily, not committed to PlayFab yet)
            var currentUser = _gameConfig.currentUserData;
            if (!string.IsNullOrEmpty(currentUser.userId))
            {
                if (mergedUsers.TryGetValue(currentUser.userId, out var existing))
                {
                    // Update if new score is higher
                    // if (currentUser.score > existing.score)
                    // {
                    //     existing.score = currentUser.score;
                    //     existing.userName = currentUser.userName;
                    // }
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
    
            // Save locally but NOT to PlayFab yet
            // _scriptable.leaderBoardUsers = userDataList.Value;
            // await _manager.SaveLeaderboardToJsonAsync();
    
            Debug.Log($"Leaderboard initialized. Total: {totalUserCount.Value}, Rank: {currentPlayerRank.Value}");
        }
    }
}