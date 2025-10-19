using UniRx;
using UnityEngine;

namespace Systems.GameSystem.Config
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Configs/GameConfig")]
    public class GameConfig : ScriptableObject
    {
        public ReactiveProperty<int> gameSpeed;
        public ReactiveProperty<float> timer;
        public ReactiveProperty<bool> hasGameStarted;
        public ReactiveProperty<bool> hasTimerStarted;
    
        // NEW: Max enemies setting
        public ReactiveProperty<int> maxEnemies;
    }
}