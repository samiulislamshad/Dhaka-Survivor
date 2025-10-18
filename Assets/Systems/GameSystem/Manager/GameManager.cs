using System;
using Cysharp.Threading.Tasks;
using Systems.EnemySystem.Controller;
using UnityEngine;
using Zenject;

namespace Systems.GameSystem.Manager
{
    public class GameManager : MonoBehaviour
    {
        private EnemyController _enemyController;
        
        // [SerializeField] private 
        
        [SerializeField] private bool hasGameStarted;
        [SerializeField] private bool hasTimerStarted;

        [SerializeField] private float timer;
        
        [Inject]
        public void InitializeDiReference(EnemyController enemyController)
        {
            _enemyController = enemyController;
        }

        private void Awake()
        {
            hasGameStarted = false;
            hasTimerStarted = false;
        }

        private async UniTask InitializeGame()
        {
            // load all environment assets
            // show Press Any Key UI
        }

        private void StartGame()
        {
            if(hasGameStarted) return;
            
        }
        
        public void Dispose()
        {
            
        }
    }
}