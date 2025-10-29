using System;

namespace Systems.InputSystem.Model
{
    [Serializable]
    public class UserData
    {
        public int rank;
        public string userName;
        public int score;
        public string userId;
        public string date;
        public string time;
        public bool isCurrentPlayer;
    
        public UserData(int rank, string userName, int score, string userId = "", bool isCurrentPlayer = false)
        {
            this.rank = rank;
            this.userName = userName;
            this.score = score;
            this.userId = userId;
            this.isCurrentPlayer = isCurrentPlayer;
        }
    }
}