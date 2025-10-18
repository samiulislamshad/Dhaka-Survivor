using Systems.EnemySystem.Enum;
using Systems.EnemySystem.Interface;
using UnityEngine;

namespace Systems.EnemySystem.Model
{
    public abstract class Enemy : MonoBehaviour, IEnemy
    {
        public abstract EnemyType Type { get; }
    
        protected float lifetime = 5f;
        protected float timer;

        public virtual void Initialize(Vector3 position)
        {
            transform.position = position;
            timer = 0f;
            gameObject.SetActive(true);
        }

        private void Update()
        {
            timer += Time.deltaTime;
            OnUpdate();
        }

        protected abstract void OnUpdate();

        public bool ShouldDespawn()
        {
            return timer >= lifetime;
        }
    }
}