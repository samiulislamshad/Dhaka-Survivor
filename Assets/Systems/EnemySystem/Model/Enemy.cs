using Systems.EnemySystem.Enum;
using Systems.EnemySystem.Interface;
using UnityEngine;
using Zenject;

namespace Systems.EnemySystem.Model
{
    public abstract class Enemy : MonoBehaviour, IEnemy, IPoolable<IMemoryPool>
    {
        [SerializeField] protected float moveSpeed = 5f;
    
        protected IMemoryPool Pool;
        public abstract EnemyType Type { get; }

        public void OnSpawned(IMemoryPool pool)
        {
            Pool = pool;
            gameObject.SetActive(true);
        
            OnEnemySpawned();
        }

        public void OnDespawned()
        {
            Pool = null;
            gameObject.SetActive(false);
        
            OnEnemyDespawned();
        }

        public void Initialize(Vector3 position)
        {
            transform.position = position;
            transform.rotation = Quaternion.identity;
        }

        public virtual void TakeDamage(float damage)
        {
            Die();
        }

        protected virtual void Die()
        {
            Pool?.Despawn(this);
        }
        
        protected virtual void OnEnemySpawned() { }
        protected virtual void OnEnemyDespawned() { }
    }
}