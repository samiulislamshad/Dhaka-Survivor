using System;
using System.Collections.Generic;
using Systems.EnemySystem.Enum;
using Systems.EnemySystem.Model;
using Systems.EnemySystem.Service;
using Systems.EnemySystem.Signals;
using Systems.GameSystem.Config;
using Systems.ParallaxSystem.View;
using UniRx;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace Systems.EnemySystem.Controller
{
    [Serializable]
    public class EnemyController : IDisposable, IFixedTickable
    {
        private readonly SignalBus _signalBus;
        
        private GameConfig _config;
        private ParallaxEnvironmentView _parallaxEnvironmentView;

        private EnemySpawner _spawner;
        private CompositeDisposable _disposable;

        private List<Enemy> _activeEnemies;
        private List<int> _lockedEnemies;
        private List<int> _unlockedEnemies;

        private float _spawnTimer;
        private float _nextSpawnTime;

        public EnemyController(GameConfig config, EnemySpawner spawner, ParallaxEnvironmentView parallaxEnvironmentView, SignalBus signalBus)
        {
            _config = config;
            _spawner = spawner;
            _parallaxEnvironmentView = parallaxEnvironmentView;
            _signalBus = signalBus;

            _disposable = new CompositeDisposable();

            _activeEnemies = new List<Enemy>();
            _lockedEnemies = new List<int> { 1, 2 };
            _unlockedEnemies = new List<int> { 0 };

            _spawnTimer = 0f;
            _nextSpawnTime = Random.Range(0.5f, 2f);

            SubscribeToProperties();
        }

        public void FixedTick()
        {
            Debug.Log($"spawnTimer: {_spawnTimer},  nextSpawnTime: {_nextSpawnTime}, enemies: {_activeEnemies.Count}");
            if (!_config.hasGameStarted.Value || !_config.hasTimerStarted.Value) return;

            _spawnTimer += Time.fixedDeltaTime;

            if (_spawnTimer >= _nextSpawnTime)
            {
                if (_activeEnemies.Count < _config.maxEnemies.Value)
                {
                    SpawnEnemy();
                }

                _spawnTimer = 0f;
                _nextSpawnTime = Random.Range(0.5f, 2f);
            }
        }

        private void SubscribeToProperties()
        {
            // Optional: Adjust spawn rate based on game speed
            _config.gameSpeed
                .Subscribe(speed =>
                {
                    // Can add difficulty scaling here if needed
                })
                .AddTo(_disposable);
            
            _signalBus.Subscribe<UnregisterEnemySignal>(UnRegisterEnemy);
        }

        #region Register and Unregister

        public void RegisterEnemy(Enemy enemy)
        {
            if (_activeEnemies.Contains(enemy)) return;
            _activeEnemies.Add(enemy);
            Debug.Log($"Enemy registered. Active count: {_activeEnemies.Count}");
        }

        public void UnRegisterEnemy(UnregisterEnemySignal signal)
        {
            var enemy = signal.enemy;
            if (!_activeEnemies.Contains(enemy)) return;
            _activeEnemies.Remove(enemy);
            Debug.Log($"Enemy unregistered. Active count: {_activeEnemies.Count}");
        }

        #endregion

        public void SpawnEnemy()
        {
            if (_unlockedEnemies.Count == 0) return;

            var randomIndex = Random.Range(0, _unlockedEnemies.Count);
            var enemyType = (EnemyType)_unlockedEnemies[randomIndex];
            var enemy = _spawner.Spawn(enemyType, _parallaxEnvironmentView.spawnPoint.position);

            if (enemy == null) return;
            RegisterEnemy(enemy);
        }

        public void Dispose()
        {
            _disposable?.Dispose();
        }
    }
}