using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Systems.GameSystem.Config;
using Systems.InputSystem.Model;
using Systems.LeaderBoardSystem.Manager;
using Systems.LeaderBoardSystem.Scriptable;
using UniRx;
using UnityEngine;
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
            var currentUser = _gameConfig.currentUserData;
            if (currentUser == null || string.IsNullOrEmpty(currentUser.userId))
                return;

            // Same Custom ID as Initialize — do not call AddNewPlayerToLeaderboard (that logs in as a new random account).
            if (!await _manager.LoginPlayer(currentUser.userId))
                return;

            var playerData = GetCurrentPlayerData();
            if (playerData == null)
                return;

            await _manager.AddPlayerToLeaderboard(playerData.userName, playerData.score);
        }

        public async UniTask InitializeLeaderBoardData()
        {
            var currentUser = _gameConfig.currentUserData;
            if (currentUser == null)
            {
                userDataList.Value = new List<UserData>();
                totalUserCount.Value = 0;
                currentPlayerRank.Value = -1;
                return;
            }

            // Stable PlayFab Custom ID must match across sessions (game userId is set at name entry).
            if (string.IsNullOrEmpty(currentUser.userId))
                currentUser.userId = Guid.NewGuid().ToString();

            if (!await _manager.LoginPlayer(currentUser.userId))
            {
                Debug.LogError("PlayFab login failed; leaderboard will be empty.");
                userDataList.Value = new List<UserData>();
                totalUserCount.Value = 0;
                currentPlayerRank.Value = -1;
                return;
            }

            var playFabId = _manager.CurrentPlayFabId;

            // Uses existing session from LoginPlayer above (no random anonymous login).
            var onlineLeaderboard = await _manager.FetchLeaderboard();

            var mergedUsers = new Dictionary<string, UserData>();
            foreach (var user in onlineLeaderboard)
            {
                if (!string.IsNullOrEmpty(user.userId))
                    mergedUsers[user.userId] = user;
            }

            // Keys in mergedUsers are PlayFabIds from GetLeaderboard — never the game's Guid alone.
            if (!string.IsNullOrEmpty(playFabId))
            {
                if (mergedUsers.TryGetValue(playFabId, out var existing))
                {
                    if (currentUser.score > existing.score)
                        existing.score = currentUser.score;
                    existing.userName = currentUser.userName;
                    existing.date = DateTime.Now.ToString("yyyy-MM-dd");
                    existing.time = DateTime.Now.ToString("HH:mm:ss");
                }
                else
                {
                    var newUser = currentUser.CloneViaSerialization();
                    newUser.userId = playFabId;
                    newUser.isCurrentPlayer = true;
                    newUser.date = DateTime.Now.ToString("yyyy-MM-dd");
                    newUser.time = DateTime.Now.ToString("HH:mm:ss");
                    mergedUsers[playFabId] = newUser;
                }
            }

            userDataList.Value = mergedUsers.Values.OrderByDescending(u => u.score).ToList();

            for (var i = 0; i < userDataList.Value.Count; i++)
            {
                userDataList.Value[i].rank = i + 1;
                var isMe = !string.IsNullOrEmpty(playFabId) &&
                           userDataList.Value[i].userId == playFabId;
                userDataList.Value[i].isCurrentPlayer = isMe;
                if (isMe)
                    currentPlayerRank.Value = userDataList.Value[i].rank;
            }

            totalUserCount.Value = userDataList.Value.Count;

            // Push score to PlayFab so the title statistic / legacy leaderboard updates (not only on Main Menu).
            if (!string.IsNullOrEmpty(playFabId))
                await _manager.AddPlayerToLeaderboard(currentUser.userName, currentUser.score);
        }
    }
}