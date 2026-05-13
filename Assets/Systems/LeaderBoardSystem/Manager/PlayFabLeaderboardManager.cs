using System;
using System.Collections.Generic;
using System.Linq;
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

        /// <summary>Public user data key for the human-readable nickname (readable by other clients via GetUserData).</summary>
        private const string LeaderboardNicknameUserDataKey = "lb_nick";

        /// <summary>Serializes display-name + user data + statistic writes so concurrent calls cannot reorder on the wire.</summary>
        private readonly SemaphoreSlim _leaderboardWriteLock = new(1, 1);

        /// <summary>PlayFab title display name must be 3–25 characters (see UpdateUserTitleDisplayName).</summary>
        private const int MinTitleDisplayNameLength = 3;
        private const int MaxTitleDisplayNameLength = 25;

        private const int MaxNicknameLength = 200;

        /// <summary>Parallel GetUserData calls per wave (avoid client burst limits).</summary>
        private const int NicknameFetchParallelism = 8;

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

            if (string.IsNullOrEmpty(customId))
                customId = GenerateUniquePlayerId();

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

        private string GenerateUniquePlayerId()
        {
            string deviceId = SystemInfo.deviceUniqueIdentifier;
            string timestamp = DateTime.UtcNow.Ticks.ToString();
            string random = UnityEngine.Random.Range(1000, 9999).ToString();

            return $"{deviceId}_{timestamp}_{random}";
        }

        /// <summary>
        /// Fetches the leaderboard and fills <see cref="UserData.userName"/> with each player's public nickname (user data),
        /// falling back to title display name when missing (legacy rows).
        /// </summary>
        public async UniTask<List<UserData>> FetchLeaderboard(int maxResults = 100)
        {
            if (!isLoggedIn)
            {
                Debug.Log("Not logged in, attempting to login for leaderboard fetch...");
                if (!await LoginPlayer())
                {
                    Debug.LogError("Failed to login for leaderboard fetch");
                    return new List<UserData>();
                }
            }

            var entries = await GetLeaderboardEntriesAsync(maxResults);
            if (entries.Count == 0)
                return new List<UserData>();

            var ids = entries.Select(e => e.PlayFabId).Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList();
            var nickById = await FetchPublicNicknamesAsync(ids);

            var leaderboardData = new List<UserData>(entries.Count);
            foreach (var entry in entries)
            {
                var nick = !string.IsNullOrEmpty(entry.PlayFabId) && nickById.TryGetValue(entry.PlayFabId, out var n) && !string.IsNullOrEmpty(n)
                    ? n
                    : (entry.DisplayName ?? "Player");

                var userData = new UserData(
                    rank: entry.Position + 1,
                    userName: nick,
                    score: entry.StatValue,
                    userId: entry.PlayFabId,
                    isCurrentPlayer: entry.PlayFabId == currentPlayerId
                );

                userData.date = "";
                userData.time = "";

                leaderboardData.Add(userData);
            }

            Debug.Log($"Fetched {leaderboardData.Count} leaderboard entries (nicknames from public user data where available)");
            return leaderboardData;
        }

        private async UniTask<List<PlayerLeaderboardEntry>> GetLeaderboardEntriesAsync(int maxResults)
        {
            var tcs = new UniTaskCompletionSource<List<PlayerLeaderboardEntry>>();

            var request = new GetLeaderboardRequest
            {
                StatisticName = "HighScore",
                StartPosition = 0,
                MaxResultsCount = maxResults
            };

            PlayFabClientAPI.GetLeaderboard(request,
                result => { tcs.TrySetResult(result.Leaderboard ?? new List<PlayerLeaderboardEntry>()); },
                error =>
                {
                    Debug.LogError($"Failed to fetch leaderboard: {error.GenerateErrorReport()}");
                    tcs.TrySetResult(new List<PlayerLeaderboardEntry>());
                });

            return await tcs.Task;
        }

        private async UniTask<Dictionary<string, string>> FetchPublicNicknamesAsync(List<string> playFabIds)
        {
            var dict = new Dictionary<string, string>(playFabIds.Count);
            for (var i = 0; i < playFabIds.Count; i += NicknameFetchParallelism)
            {
                var chunk = playFabIds.Skip(i).Take(NicknameFetchParallelism).ToArray();
                var tasks = chunk.Select(GetPublicNicknameForPlayerAsync).ToArray();
                var values = await UniTask.WhenAll(tasks);
                for (var j = 0; j < chunk.Length; j++)
                {
                    if (!string.IsNullOrEmpty(values[j]))
                        dict[chunk[j]] = values[j];
                }
            }

            return dict;
        }

        private async UniTask<string> GetPublicNicknameForPlayerAsync(string playFabId)
        {
            if (string.IsNullOrEmpty(playFabId))
                return null;

            var tcs = new UniTaskCompletionSource<string>();

            var request = new GetUserDataRequest
            {
                PlayFabId = playFabId,
                Keys = new List<string> { LeaderboardNicknameUserDataKey }
            };

            PlayFabClientAPI.GetUserData(request,
                result =>
                {
                    if (result.Data != null &&
                        result.Data.TryGetValue(LeaderboardNicknameUserDataKey, out var rec) &&
                        rec != null &&
                        !string.IsNullOrEmpty(rec.Value))
                        tcs.TrySetResult(rec.Value);
                    else
                        tcs.TrySetResult(null);
                },
                _ => { tcs.TrySetResult(null); });

            return await tcs.Task;
        }

        /// <summary>
        /// Pushes score and stores nickname: title display name is a unique handle (PlayFab id–based); visible name lives in public user data.
        /// </summary>
        /// <param name="nickname">Human-readable name (same as in-game <c>currentUser.userName</c>).</param>
        public async UniTask<bool> AddPlayerToLeaderboard(string nickname, int score)
        {
            if (!isLoggedIn)
            {
                Debug.LogError("Must call LoginPlayer() first before adding to leaderboard!");
                return false;
            }

            await _leaderboardWriteLock.WaitAsync();
            try
            {
                var uniqueHandle = BuildUniqueTitleDisplayName(currentPlayerId);
                if (!await SetTitleDisplayNameAsync(uniqueHandle))
                {
                    Debug.LogError("Failed to set unique title display name; aborting leaderboard write.");
                    return false;
                }

                if (!await SetPublicNicknameUserDataAsync(nickname))
                {
                    Debug.LogError("Failed to write public nickname user data; aborting statistic update.");
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
                        Debug.Log($"Leaderboard updated: nick='{SanitizeNickname(nickname)}', score={score}, handle='{uniqueHandle}'");
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

        public async UniTask<bool> AddNewPlayerToLeaderboard(string playerName, int score)
        {
            if (!await LoginPlayer())
            {
                Debug.LogError("Failed to login new player");
                return false;
            }

            return await AddPlayerToLeaderboard(playerName, score);
        }

        public async UniTask<bool> RemovePlayerFromLeaderboard()
        {
            if (!isLoggedIn)
            {
                Debug.LogError("Not logged in!");
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
                        Value = 0
                    }
                }
            };

            PlayFabClientAPI.UpdatePlayerStatistics(request,
                _ =>
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

        public async UniTask<UserData> GetCurrentPlayerData()
        {
            if (!isLoggedIn)
            {
                Debug.LogError("Not logged in!");
                return null;
            }

            var entry = await GetLeaderboardAroundPlayerEntryAsync();
            if (entry == null)
                return null;

            var nick = await GetPublicNicknameForPlayerAsync(entry.PlayFabId);
            var displayNick = !string.IsNullOrEmpty(nick) ? nick : (entry.DisplayName ?? "Player");

            return new UserData(
                rank: entry.Position + 1,
                userName: displayNick,
                score: entry.StatValue,
                userId: entry.PlayFabId,
                isCurrentPlayer: true
            )
            {
                date = "",
                time = ""
            };
        }

        private async UniTask<PlayerLeaderboardEntry> GetLeaderboardAroundPlayerEntryAsync()
        {
            var tcs = new UniTaskCompletionSource<PlayerLeaderboardEntry>();

            var request = new GetLeaderboardAroundPlayerRequest
            {
                StatisticName = "HighScore",
                MaxResultsCount = 1
            };

            PlayFabClientAPI.GetLeaderboardAroundPlayer(request,
                result =>
                {
                    if (result.Leaderboard != null && result.Leaderboard.Count > 0)
                        tcs.TrySetResult(result.Leaderboard[0]);
                    else
                        tcs.TrySetResult(null);
                },
                error =>
                {
                    Debug.LogError($"Failed to get player data: {error.GenerateErrorReport()}");
                    tcs.TrySetResult(null);
                });

            return await tcs.Task;
        }

        /// <summary>Title display name unique per account; not shown as the player nickname in UI.</summary>
        private static string BuildUniqueTitleDisplayName(string playFabId)
        {
            if (string.IsNullOrEmpty(playFabId))
                return "id_unknown____";

            var s = playFabId.Length <= MaxTitleDisplayNameLength
                ? playFabId
                : playFabId.Substring(0, MaxTitleDisplayNameLength);

            if (s.Length < MinTitleDisplayNameLength)
                s = (s + new string('x', MinTitleDisplayNameLength)).Substring(0, MinTitleDisplayNameLength);

            return s;
        }

        private static string SanitizeNickname(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return "Player";
            var t = raw.Trim();
            if (t.Length > MaxNicknameLength)
                t = t.Substring(0, MaxNicknameLength);
            return t;
        }

        private async UniTask<bool> SetTitleDisplayNameAsync(string displayName)
        {
            var tcs = new UniTaskCompletionSource<bool>();

            var request = new UpdateUserTitleDisplayNameRequest
            {
                DisplayName = displayName
            };

            PlayFabClientAPI.UpdateUserTitleDisplayName(request,
                result =>
                {
                    Debug.Log($"Title display handle set to: {result.DisplayName}");
                    tcs.TrySetResult(true);
                },
                error =>
                {
                    Debug.LogError($"Failed to set title display name: {error.GenerateErrorReport()}");
                    tcs.TrySetResult(false);
                });

            return await tcs.Task;
        }

        private async UniTask<bool> SetPublicNicknameUserDataAsync(string nickname)
        {
            var tcs = new UniTaskCompletionSource<bool>();
            var value = SanitizeNickname(nickname);

            var request = new UpdateUserDataRequest
            {
                Data = new Dictionary<string, string>
                {
                    { LeaderboardNicknameUserDataKey, value }
                },
                Permission = UserDataPermission.Public
            };

            PlayFabClientAPI.UpdateUserData(request,
                _ =>
                {
                    Debug.Log($"Public nickname user data set ({LeaderboardNicknameUserDataKey})");
                    tcs.TrySetResult(true);
                },
                error =>
                {
                    Debug.LogError($"Failed to set public nickname user data: {error.GenerateErrorReport()}");
                    tcs.TrySetResult(false);
                });

            return await tcs.Task;
        }
    }
}
