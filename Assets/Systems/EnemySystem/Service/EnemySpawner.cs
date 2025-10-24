using System;
using Systems.EnemySystem.Enum;
using Systems.EnemySystem.Model;
using Systems.EnemySystem.ObjectPool;
using Systems.EnemySystem.Signals;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace Systems.EnemySystem.Service
{
    [Serializable]
    public class EnemySpawner : ITickable
    {
        private readonly SignalBus _signalBus;
        
        private readonly Chesra1Pool _chesra1Pool;
        private readonly ChapriBikerPool _chapriBikerPool;
        private readonly Chesra2Pool _chesra2Pool;
        private readonly Chesra3Pool _chesra3Pool;
        private readonly OfficeBossPool _officeBossPool;
        private readonly AuntyPool _auntyPool;
        private readonly MinarPool _minarPool;
        private readonly PagriBroPool _pagriBroPool;
        
        private UnregisterEnemySignal _unregisterEnemySignal;

        public EnemySpawner(Chesra1Pool chesra1Pool, 
            ChapriBikerPool chapriBikerPool, 
            Chesra2Pool chesra2Pool,
            Chesra3Pool chesra3Pool, 
            OfficeBossPool officeBossPool, 
            AuntyPool auntyPool, 
            MinarPool minarPool, 
            PagriBroPool pagriBroPool,
            SignalBus signalBus)
        {
            _chesra1Pool = chesra1Pool;
            _chapriBikerPool = chapriBikerPool;
            _chesra2Pool = chesra2Pool;
            _signalBus = signalBus;
            _chesra3Pool = chesra3Pool;
            _officeBossPool = officeBossPool;
            _auntyPool = auntyPool;
            _minarPool = minarPool;
            _pagriBroPool = pagriBroPool;

            _unregisterEnemySignal = new UnregisterEnemySignal(null);
        }

        public Enemy Spawn(EnemyType type, Vector3 position)
        {
            switch (type)
            {
                case EnemyType.Chesra1:
                    return _chesra1Pool.Spawn(position);
                    
                case EnemyType.ChapriBiker:
                    return _chapriBikerPool.Spawn(position);
                    
                case EnemyType.Chesra2:
                    return _chesra2Pool.Spawn(position);
                case EnemyType.OfficeBoss:
                    return _officeBossPool.Spawn(position);
                case EnemyType.Aunty:
                    return _auntyPool.Spawn(position);
                case EnemyType.Hojor:
                    return _pagriBroPool.Spawn(position);
                case EnemyType.Chesra3:
                    return _chesra3Pool.Spawn(position);
                case EnemyType.Minar:
                    return _minarPool.Spawn(position);
            }

            return null;
        }

        public void Tick()
        {
            CleanPool(_chesra1Pool);
            CleanPool(_chapriBikerPool);
            CleanPool(_chesra2Pool);
            CleanPool(_officeBossPool);
            CleanPool(_minarPool);
            CleanPool(_auntyPool);
            CleanPool(_pagriBroPool);
            CleanPool(_chesra3Pool);
        }

        private void CleanPool<T>(MonoMemoryPool<Vector3, T> pool) where T : Enemy
        {
            var enemies = Object.FindObjectsOfType<T>();
            foreach (var enemy in enemies)
            {
                if (enemy.gameObject.activeSelf && enemy.ShouldDespawn())
                {
                    pool.Despawn(enemy);
                    _unregisterEnemySignal.enemy = enemy;
                    _signalBus.Fire(_unregisterEnemySignal);
                }
            }
        }
    }
}