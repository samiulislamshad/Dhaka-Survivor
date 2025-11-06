using Systems.InputSystem.Model;
using UniRx;
using UnityEngine;

namespace Systems.GameSystem.Config
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Configs/GameConfig")]
    public class GameConfig : ScriptableObject
    {
        public ReactiveProperty<GamePhase> gamePhase;
        public ReactiveProperty<bool> isRetrying = new();
        
        public ReactiveProperty<float> gameSpeed;
        public ReactiveProperty<float> timer;
        public ReactiveProperty<bool> hasGameStarted;
        public ReactiveProperty<bool> hasTimerStarted;

        public UserData currentUserData;
    
        // NEW: Max enemies setting
        public ReactiveProperty<int> maxEnemies;
        public float enemySpeedMultiplier;
    }
}