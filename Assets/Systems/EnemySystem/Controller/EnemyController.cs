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

        private Dictionary<string, int> _enemySpeechBubbles;

        private float _spawnTimer;
        private const float InitialSpawnTime = 3f;
        private float _changeRate = 10;
        private float _spawnRate = 0.8f;
        private float _nextSpawnTime;

        public EnemyController(GameConfig config, EnemySpawner spawner, ParallaxEnvironmentView parallaxEnvironmentView,
            SignalBus signalBus)
        {
            _config = config;
            _spawner = spawner;
            _parallaxEnvironmentView = parallaxEnvironmentView;
            _signalBus = signalBus;

            _disposable = new CompositeDisposable();

            _activeEnemies = new List<Enemy>();
            _lockedEnemies = new List<int> { 1 };
            _unlockedEnemies = new List<int> { 0, 2, 3, 4, 5, 6, 7 };

            _enemySpeechBubbles = new Dictionary<string, int>
            {
                ["Aunty"] = 2, ["OfficeBoss"] = 2, ["Chesra1"] = 3, ["Chesra2"] = 4, ["Chesra3"] = 5,
            };

            _spawnTimer = 0f;
            _nextSpawnTime = 2f;

            SubscribeToProperties();
        }

        public void FixedTick()
        {
            if (!_config.hasGameStarted.Value || !_config.hasTimerStarted.Value) return;

            ModifySpawnTimer();
            _spawnTimer += Time.fixedDeltaTime;

            if (_spawnTimer >= _nextSpawnTime)
            {
                if (_activeEnemies.Count < _config.maxEnemies.Value)
                {
                    SpawnEnemy();
                }

                _spawnTimer = 0f;
            }
        }

        private void ModifySpawnTimer()
        {
            var steps = (int)(_config.timer.Value / _changeRate);
            var baseSpawnTime = InitialSpawnTime * Mathf.Pow(_spawnRate, steps);
    
            // Add ±20% variability
            _nextSpawnTime = baseSpawnTime * Random.Range(0.8f, 1.2f);
        }

        private void SubscribeToProperties()
        {
            Observable.EveryUpdate().Subscribe(value =>
            {
                if (value <= 50) return;
                if (_lockedEnemies.Count <= 0) return;
                _unlockedEnemies.Add(_lockedEnemies[0]);
                _lockedEnemies.RemoveAt(0);
            }).AddTo(_disposable);

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
            
            var groupSize = _config.timer.Value >= 30f ? Random.Range(1, 4) : 1;
            for (var i = 0; i < groupSize; i++)
            {
                var randomIndex = Random.Range(0, _unlockedEnemies.Count);
                var enemyType = (EnemyType)_unlockedEnemies[randomIndex];
                
                var spawnPosition = _parallaxEnvironmentView.firstLayerSpawnPoint.position;
                spawnPosition.x += i * 2f;
                var enemy = _spawner.Spawn(enemyType, spawnPosition);
                if (enemy == null) continue;
                // var enemyName = enemy.GetEnemyName();
                // if (_enemySpeechBubbles.TryGetValue(enemyName, out var count))
                // {
                //     if (count > 0)
                //     {
                //         enemy.canShowSpeechBubble = true;
                //         _enemySpeechBubbles[enemyName]--;
                //     }
                // }
                
                RegisterEnemy(enemy);
            }
        }
        
        public void Dispose()
        {
            _disposable?.Dispose();
        }
    }
}