using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using Systems.InputSystem.Model;
using Systems.LeaderBoardSystem.Model;
using Systems.LeaderBoardSystem.Scriptable;
using UnityEngine;

namespace Systems.LeaderBoardSystem.Manager
{
    [Serializable]
    public class LeaderboardManager
    {
        private readonly LeaderBoardScriptable _leaderBoardScriptable;
        private readonly string _filePath;
        private const string FileName = "leaderBoard.json";

        // Constructor for Zenject injection
        public LeaderboardManager(LeaderBoardScriptable leaderBoardScriptable)
        {
            _leaderBoardScriptable = leaderBoardScriptable;
            _filePath = Path.Combine(Application.persistentDataPath, FileName);
            Debug.Log("Leaderboard file path: " + _filePath);
        }

        // Read JSON and populate the ScriptableObject (Async)
        public async UniTask LoadLeaderBoardFromJsonAsync()
        {
            if (File.Exists(_filePath))
            {
                try
                {
                    // Read file asynchronously
                    string jsonData = await File.ReadAllTextAsync(_filePath);

                    // Parse on background thread to avoid blocking
                    var wrapper = await UniTask.RunOnThreadPool(() =>
                        JsonUtility.FromJson<LeaderBoardWrapper>(jsonData)
                    );

                    _leaderBoardScriptable.leaderBoardUsers = wrapper.users;
                    Debug.Log("Leaderboard loaded successfully. Total users: " +
                              _leaderBoardScriptable.leaderBoardUsers.Count);
                }
                catch (Exception e)
                {
                    Debug.LogError("Error loading leaderboard: " + e.Message);
                }
            }
            else
            {
                Debug.LogWarning("Leaderboard file not found. Creating new leaderboard.");
                _leaderBoardScriptable.leaderBoardUsers = new List<UserData>();
            }
        }

        // Write ScriptableObject data to JSON (Async)
        public async UniTask SaveLeaderboardToJsonAsync()
        {
            try
            {
                LeaderBoardWrapper wrapper = new LeaderBoardWrapper
                {
                    users = _leaderBoardScriptable.leaderBoardUsers
                };

                // Serialize on background thread
                string jsonData = await UniTask.RunOnThreadPool(() =>
                    JsonUtility.ToJson(wrapper, true)
                );

                // Write file asynchronously
                await File.WriteAllTextAsync(_filePath, jsonData);
                Debug.Log("Leaderboard saved successfully to: " + _filePath);
            }
            catch (Exception e)
            {
                Debug.LogError("Error saving leaderboard: " + e.Message);
            }
        }

        // Example: Add a new user and save
        // public async UniTask AddUserAsync(string userName, int score, string userId = "",
        //     bool isCurrentPlayer = false)
        // {
        //     int newRank = _leaderBoardScriptable.leaderBoardUsers.Count + 1;
        //     UserData newUser = new UserData(newRank, userName, score, userId, isCurrentPlayer);
        //
        //     newUser.date = System.DateTime.Now.ToString("yyyy-MM-dd");
        //     newUser.time = System.DateTime.Now.ToString("HH:mm:ss");
        //
        //     _leaderBoardScriptable.leaderBoardUsers.Add(newUser);
        //     await SaveLeaderboardToJsonAsync();
        // }

        // Synchronous versions if needed
        public void LoadLeaderboardFromJson()
        {
            LoadLeaderBoardFromJsonAsync().Forget();
        }

        public void SaveLeaderboardToJson()
        {
            SaveLeaderboardToJsonAsync().Forget();
        }

        // Helper wrapper class for JSON serialization
    }
}