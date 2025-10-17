using Systems.EnemySystem.Model;
using Zenject;

namespace Systems.EnemySystem.EnemyFactory
{
    public abstract class EnemyPool<T> : MemoryPool<T> where T : Enemy
    {
        protected override void OnSpawned(T enemy)
        {
            base.OnSpawned(enemy);
            enemy.OnSpawned(this);
        }

        protected override void OnDespawned(T enemy)
        {
            base.OnDespawned(enemy);
            enemy.OnDespawned();
        }
    }
}