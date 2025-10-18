using System;
using System.Collections.Generic;
using Systems.EnemySystem.Config;
using Systems.EnemySystem.Enum;
using Systems.EnemySystem.Model;
using Systems.EnemySystem.Service;
using UniRx;
using Random = UnityEngine.Random;

namespace Systems.EnemySystem.Controller
{
    [Serializable]
    public class EnemyController : IDisposable
    {
        private EnemyConfig _config;
        private EnemySpawner _spawner;
        private CompositeDisposable _disposable;

        private List<Enemy> _activeEnemies;
        private List<int> _lockedEnemies;
        private List<int> _unlockedEnemies;

        public EnemyController(EnemyConfig config, EnemySpawner spawner)
        {
            _config = config;
            _spawner = spawner;
            
            _disposable = new CompositeDisposable();
            
            _activeEnemies = new List<Enemy>();
            _lockedEnemies = new List<int> {1,2};
            _unlockedEnemies = new List<int> {0};
        }

        #region Register and Unregister

        public void RegisterEnemy(Enemy enemy)
        {
            if(_activeEnemies.Contains(enemy)) return;
            _activeEnemies.Add(enemy);
        }

        public void UnRegisterEnemy(Enemy enemy)
        {
            if(!_activeEnemies.Contains(enemy)) return;
            _activeEnemies.Remove(enemy);
        }

        #endregion

        public void SpawnEnemy()
        {
            var randomEnemyType = Random.Range(0,_unlockedEnemies.Count);
            var enemyType = (EnemyType) randomEnemyType;
            var enemy = _spawner.Spawn(enemyType, _config.spawnPoint.position);
            if (enemy == null) return;
            RegisterEnemy(enemy);
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}