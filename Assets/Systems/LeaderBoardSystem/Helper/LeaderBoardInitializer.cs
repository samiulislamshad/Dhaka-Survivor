using Systems.LeaderBoardSystem.Controller;
using Systems.LeaderBoardSystem.Model;
using Systems.LeaderBoardSystem.View;
using UnityEngine;
using Zenject;

namespace Systems.LeaderBoardSystem.Helper
{
    public class LeaderBoardInitializer : MonoBehaviour
    {
        [SerializeField] private LeaderBoardCanvasView canvasView;
        
        [Inject] private LeaderBoardModel _model;
        [Inject] private LeaderBoardController _controller;

        private void Start()
        {
            // InitializeLeaderboard();
            // LoadMockData();
        }

        // private void InitializeLeaderboard()
        // {
        //     // _model = new LeaderBoardModel();
        //     // _controller = new LeaderBoardController(_model, canvasView);
        // }
        //
        // private void LoadMockData()
        // {
        //     // Create full leaderboard (500 players for example)
        //     var leaderboardData = new List<UserData>();
        //     
        //     // Add all players
        //     for (int i = 1; i <= 500; i++)
        //     {
        //         int score = 10000 - (i * 20);
        //         leaderboardData.Add(new UserData(i, $"Player{i}", score.ToString(), $"user_{i:D3}"));
        //     }
        //
        //     // Current player is at rank 247
        //     string currentPlayerId = "user_247";
        //     
        //     // Load data - will automatically show top 10 + player at rank 247 as 11th
        //     _controller.LoadLeaderboardData(leaderboardData, currentPlayerId);
        //     
        //     // Display will show:
        //     // 1. Player1
        //     // 2. Player2
        //     // ...
        //     // 10. Player10
        //     // 11. Player247 (HIGHLIGHTED)
        // }
        //
        // private void OnDestroy()
        // {
        //     _controller?.Dispose();
        // }
        //
        // // Example: Update leaderboard when player rank changes
        // public void RefreshLeaderboard()
        // {
        //     // Fetch new data from server
        //     List<UserData> newData = GetUpdatedLeaderboard();
        //     string currentPlayerId = GetCurrentPlayerId();
        //     
        //     _controller.LoadLeaderboardData(newData, currentPlayerId);
        // }
        //
        // // Mock methods - replace with your actual API calls
        // private List<UserData> GetUpdatedLeaderboard()
        // {
        //     // Your API call here
        //     return new List<UserData>();
        // }
        //
        // private string GetCurrentPlayerId()
        // {
        //     // Your player ID retrieval here
        //     return "user_247";
        // }
    }
}