using Systems.EnemySystem.Config;
using Systems.EnemySystem.Enum;
using Systems.EnemySystem.Interface;
using UnityEngine;
using Zenject;

namespace Systems.EnemySystem.Model
{
    public abstract class Enemy : MonoBehaviour, IEnemy
    {
        [Inject] protected EnemyConfig Config;
        
        public abstract EnemyType Type { get; }

        [SerializeField] protected Rigidbody2D rb;
        [SerializeField] protected LayerMask despawnLayer;
        [SerializeField] protected LayerMask playerLayer;
        
        protected bool IsDespawning;
        protected float Timer;

        public virtual void Initialize(Vector3 position)
        {
            transform.position = position;
            Timer = 0f;
            IsDespawning = false;
            gameObject.SetActive(true);
        }

        private void FixedUpdate()
        {
            Timer += Time.deltaTime;
            OnFixedUpdate();
        }

        protected abstract void OnFixedUpdate();

        public virtual bool ShouldDespawn()
        {
            return IsDespawning;
        }
    }
}