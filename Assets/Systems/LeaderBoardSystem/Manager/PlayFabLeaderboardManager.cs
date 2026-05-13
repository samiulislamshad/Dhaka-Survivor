using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using PlayFab;
using PlayFab.ClientModels;
using Systems.InputSystem.Model;
using UnityEngine;

namespace Systems.LeaderBoardSystem.Manager
{
    [Serializable]
    public class PlayFabLeaderboardManager
    {
        private string titleId = "6C475";
        private string currentPlayerId = "";
        private bool isLoggedIn = false;

        /// <summary>PlayFab player entity id for the active session (after <see cref="LoginPlayer"/>).</summary>
        public string CurrentPlayFabId => currentPlayerId;

        public bool IsLoggedIn => isLoggedIn;

        /// <summary>Serializes display-name + statistic writes so concurrent calls cannot reorder on the wire.</summary>
        private readonly SemaphoreSlim _leaderboardWriteLock = new(1, 1);

        /// <summary>PlayFab title display name length limit (keep headroom for uniqueness suffix).</summary>
        private const int MaxDisplayNameLength = 25;

        public PlayFabLeaderboardManager()
        {
            PlayFabSettings.staticSettings.TitleId = titleId;
        }

        /// <summary>
        /// Login with a unique player ID. Call this before adding scores.
        /// If no customId provided, generates a new unique ID for each player.
        /// </summary>
        /// <param name="customId">Optional: Provide specific ID to login as existing player</param>
        public async UniTask<bool> LoginPlayer(string customId = null)
        {
            var tcs = new UniTaskCompletionSource<bool>();

            // If no customId provided, generate a unique one
            if (string.IsNullOrEmpty(customId))
            {
                customId = GenerateUniquePlayerId();
            }

            var request = new LoginWithCustomIDRequest
            {
                CustomId = customId,
                CreateAccount = true
            };

            PlayFabClientAPI.LoginWithCustomID(request,
                result =>
                {
                    currentPlayerId = result.PlayFabId;
                    isLoggedIn = true;
                    Debug.Log($"PlayFab login successful! Player ID: {currentPlayerId}, CustomID: {customId}");
                    tcs.TrySetResult(true);
                },
                error =>
                {
                    Debug.LogError($"PlayFab login failed: {error.GenerateErrorReport()}");
                    tcs.TrySetResult(false);
                });

            return await tcs.Task;
        }

        /// <summary>
        /// Generates a unique player ID using device ID + timestamp + random value
        /// </summary>
        private string GenerateUniquePlayerId()
        {
            string deviceId = SystemInfo.deviceUniqueIdentifier;
            string timestamp = DateTime.UtcNow.Ticks.ToString();
            string random = UnityEngine.Random.Range(1000, 9999).ToString();

            return $"{deviceId}_{timestamp}_{random}";
        }

        /// <summary>
        /// Fetches the entire leaderboard from PlayFab and converts it to List<UserData>
        /// </summary>
        /// <param name="maxResults">Maximum number of entries to fetch (default 100)</param>
        /// <returns>List of UserData sorted by rank</returns>
        public async UniTask<List<UserData>> FetchLeaderboard(int maxResults = 100)
        {
            // Ensure we're logged in before fetching
            if (!isLoggedIn)
            {
                Debug.Log("Not logged in, attempting to login for leaderboard fetch...");
                bool loginSuccess = await LoginPlayer();
                if (!loginSuccess)
                {
                    Debug.LogError("Failed to login for leaderboard fetch");
                    return new List<UserData>();
                }
            }

            var tcs = new UniTaskCompletionSource<List<UserData>>();

            var request = new GetLeaderboardRequest
            {
                StatisticName = "HighScore",
                StartPosition = 0,
                MaxResultsCount = maxResults
            };

            PlayFabClientAPI.GetLeaderboard(request,
                result =>
                {
                    List<UserData> leaderboardData = new List<UserData>();

                    foreach (var entry in result.Leaderboard)
                    {
                        UserData userData = new UserData(
                            rank: entry.Position + 1,
                            userName: entry.DisplayName ?? "Player",
                            score: entry.StatValue,
                            userId: entry.PlayFabId,
                            isCurrentPlayer: entry.PlayFabId == currentPlayerId
                        );

                        userData.date = "";
                        userData.time = "";

                        leaderboardData.Add(userData);
                    }

                    Debug.Log($"Fetched {leaderboardData.Count} leaderboard entries");
                    tcs.TrySetResult(leaderboardData);
                },
                error =>
                {
                    Debug.LogError($"Failed to fetch leaderboard: {error.GenerateErrorReport()}");
                    tcs.TrySetResult(new List<UserData>());
                });

            return await tcs.Task;
        }

        /// <summary>
        /// Adds/updates player's score to PlayFab leaderboard
        /// IMPORTANT: You must call LoginPlayer() with a unique ID before calling this!
        /// Only updates if the new score is higher than existing score
        /// </summary>
        /// <param name="playerName">Player's display name</param>
        /// <param name="score">Player's score</param>
        /// <returns>True if successful</returns>
        public async UniTask<bool> AddPlayerToLeaderboard(string playerName, int score)
        {
            if (!isLoggedIn)
            {
                Debug.LogError("Must call LoginPlayer() first before adding to leaderboard!");
                return false;
            }

            await _leaderboardWriteLock.WaitAsync();
            try
            {
                // Must succeed before statistics: otherwise the leaderboard row keeps a default / numeric name.
                var resolvedName = await TrySetDisplayNameWithRetries(playerName);
                if (string.IsNullOrEmpty(resolvedName))
                {
                    Debug.LogError("PlayFab display name could not be set; skipping statistic update to avoid anonymous-looking rows.");
                    return false;
                }

                var tcs = new UniTaskCompletionSource<bool>();

                var request = new UpdatePlayerStatisticsRequest
                {
                    Statistics = new List<StatisticUpdate>
                    {
                        new StatisticUpdate
                        {
                            StatisticName = "HighScore",
                            Value = score
                        }
                    }
                };

                PlayFabClientAPI.UpdatePlayerStatistics(request,
                    _ =>
                    {
                        Debug.Log($"Player score submitted successfully: {resolvedName} - {score}");
                        tcs.TrySetResult(true);
                    },
                    error =>
                    {
                        Debug.LogError($"Failed to submit score: {error.GenerateErrorReport()}");
                        tcs.TrySetResult(false);
                    });

                return await tcs.Task;
            }
            finally
            {
                _leaderboardWriteLock.Release();
            }
        }

        /// <summary>
        /// Convenience method: Login a new unique player and add their score in one call
        /// </summary>
        /// <param name="playerName">Player's display name</param>
        /// <param name="score">Player's score</param>
        /// <returns>True if successful</returns>
        public async UniTask<bool> AddNewPlayerToLeaderboard(string playerName, int score)
        {
            // Login as a new unique player
            bool loginSuccess = await LoginPlayer();

            if (!loginSuccess)
            {
                Debug.LogError("Failed to login new player");
                return false;
            }

            // Add their score
            return await AddPlayerToLeaderboard(playerName, score);
        }

        /// <summary>
        /// Removes player's score from the leaderboard
        /// Note: This actually sets the score to 0, as PlayFab doesn't allow deletion
        /// </summary>
        /// <returns>True if successful</returns>
        public async UniTask<bool> RemovePlayerFromLeaderboard()
        {
            if (!isLoggedIn)
            {
                Debug.LogError("Not logged in!");
                return false;
            }

            var tcs = new UniTaskCompletionSource<bool>();

            // PlayFab doesn't support deletion, so we set score to 0
            var request = new UpdatePlayerStatisticsRequest
            {
                Statistics = new List<StatisticUpdate>
                {
                    new StatisticUpdate
                    {
                        StatisticName = "HighScore",
                        Value = 0
                    }
                }
            };

            PlayFabClientAPI.UpdatePlayerStatistics(request,
                result =>
                {
                    Debug.Log("Player score reset to 0");
                    tcs.TrySetResult(true);
                },
                error =>
                {
                    Debug.LogError($"Failed to reset score: {error.GenerateErrorReport()}");
                    tcs.TrySetResult(false);
                });

            return await tcs.Task;
        }

        /// <summary>
        /// Gets the current player's rank and score
        /// </summary>
        /// <returns>UserData for current player, or null if not found</returns>
        public async UniTask<UserData> GetCurrentPlayerData()
        {
            if (!isLoggedIn)
            {
                Debug.LogError("Not logged in!");
                return null;
            }

            var tcs = new UniTaskCompletionSource<UserData>();

            var request = new GetLeaderboardAroundPlayerRequest
            {
                StatisticName = "HighScore",
                MaxResultsCount = 1
            };

            PlayFabClientAPI.GetLeaderboardAroundPlayer(request,
                result =>
                {
                    if (result.Leaderboard.Count > 0)
                    {
                        var entry = result.Leaderboard[0];

                        UserData playerData = new UserData(
                            rank: entry.Position + 1,
                            userName: entry.DisplayName ?? "Player",
                            score: entry.StatValue,
                            userId: entry.PlayFabId,
                            isCurrentPlayer: true
                        );

                        playerData.date = "";
                        playerData.time = "";

                        tcs.TrySetResult(playerData);
                    }
                    else
                    {
                        tcs.TrySetResult(null);
                    }
                },
                error =>
                {
                    Debug.LogError($"Failed to get player data: {error.GenerateErrorReport()}");
                    tcs.TrySetResult(null);
                });

            return await tcs.Task;
        }

        private static string SanitizeDisplayNameBase(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return "Player";
            var t = raw.Trim();
            if (t.Length > MaxDisplayNameLength)
                t = t.Substring(0, MaxDisplayNameLength);
            return t;
        }

        private static string BuildDisplayNameCandidate(string baseName, int attempt)
        {
            baseName = SanitizeDisplayNameBase(baseName);
            if (attempt <= 0)
                return baseName;

            var suffix = $"_{UnityEngine.Random.Range(10000, 99999)}";
            var maxBaseLen = MaxDisplayNameLength - suffix.Length;
            if (maxBaseLen < 1)
                return suffix.TrimStart('_');
            if (baseName.Length > maxBaseLen)
                baseName = baseName.Substring(0, maxBaseLen);
            return baseName + suffix;
        }

        private static bool ShouldRetryDisplayNameWithNewCandidate(PlayFabErrorCode code)
        {
            return code == PlayFabErrorCode.ProfaneDisplayName
                   || code == PlayFabErrorCode.AllowNonUniquePlayerDisplayNamesDisableNotAllowed
                   || code == PlayFabErrorCode.InvalidDisplayNameRandomSuffixLength
                   || code == PlayFabErrorCode.InvalidUsername
                   || code == PlayFabErrorCode.InvalidParams;
        }

        /// <summary>
        /// Returns the display name PlayFab accepted, or null if all attempts failed.
        /// </summary>
        private async UniTask<string> TrySetDisplayNameWithRetries(string desiredName)
        {
            const int maxAttempts = 8;
            for (var attempt = 0; attempt < maxAttempts; attempt++)
            {
                var candidate = BuildDisplayNameCandidate(desiredName, attempt);
                var (ok, err) = await SetPlayerNameAsync(candidate);
                if (ok)
                    return candidate;

                if (err == null)
                    break;

                Debug.LogWarning($"Display name '{candidate}' rejected: {err.GenerateErrorReport()}");

                if (ShouldRetryDisplayNameWithNewCandidate(err.Error))
                    continue;

                if (err.Error == PlayFabErrorCode.ConnectionError && attempt + 1 < maxAttempts)
                {
                    await UniTask.Delay(TimeSpan.FromMilliseconds(250));
                    continue;
                }

                break;
            }

            return null;
        }

        private async UniTask<(bool success, PlayFabError error)> SetPlayerNameAsync(string name)
        {
            var tcs = new UniTaskCompletionSource<(bool, PlayFabError)>();

            var request = new UpdateUserTitleDisplayNameRequest
            {
                DisplayName = name
            };

            PlayFabClientAPI.UpdateUserTitleDisplayName(request,
                result =>
                {
                    Debug.Log($"Display name set to: {result.DisplayName}");
                    tcs.TrySetResult((true, null));
                },
                error => { tcs.TrySetResult((false, error)); });

            return await tcs.Task;
        }
    }
}