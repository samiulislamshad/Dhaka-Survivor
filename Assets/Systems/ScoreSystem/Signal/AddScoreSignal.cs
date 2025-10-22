using System;

namespace Systems.ScoreSystem.Signal
{
    [Serializable]
    public class AddScoreSignal
    {
        public int score;

        public AddScoreSignal(int score)
        {
            this.score = score;
        }
    }
}