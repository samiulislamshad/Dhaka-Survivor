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
        private readonly MeleeEnemyPool _meleePool;
        private readonly RangedEnemyPool _rangedPool;
        private readonly AerialEnemyPool _aerialPool;

        private UnregisterEnemySignal _unregisterEnemySignal;

        public EnemySpawner(MeleeEnemyPool meleePool, RangedEnemyPool rangedPool, AerialEnemyPool aerialPool, SignalBus signalBus)
        {
            _meleePool = meleePool;
            _rangedPool = rangedPool;
            _aerialPool = aerialPool;
            _signalBus = signalBus;

            _unregisterEnemySignal = new UnregisterEnemySignal(null);
        }

        public Enemy Spawn(EnemyType type, Vector3 position)
        {
            switch (type)
            {
                case EnemyType.Melee:
                    return _meleePool.Spawn(position);
                    
                case EnemyType.Ranged:
                    return _rangedPool.Spawn(position);
                    
                case EnemyType.Aerial:
                    return _aerialPool.Spawn(position);
            }

            return null;
        }

        public void Tick()
        {
            CleanPool(_meleePool);
            CleanPool(_rangedPool);
            CleanPool(_aerialPool);
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