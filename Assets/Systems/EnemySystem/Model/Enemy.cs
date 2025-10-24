using Systems.EnemySystem.Enum;
using Systems.EnemySystem.Interface;
using Systems.GameSystem.Config;
using UnityEngine;
using Zenject;

namespace Systems.EnemySystem.Model
{
    public abstract class Enemy : MonoBehaviour, IEnemy
    {
        [Inject] protected GameConfig Config;
        [Inject] protected SignalBus SignalBus;

        public abstract EnemyType Type { get; }
        public Animator animator;

        [SerializeField] protected Rigidbody2D rb;
        
        protected bool IsDespawning;
        protected float Timer;
        
        [Header("Movement Settings")]
        [SerializeField] protected float moveSpeedX = 1f;
        [SerializeField] protected float moveSpeedY;

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
        
        protected float GetCalculatedSpeedX()
        {
            return moveSpeedX * Config.gameSpeed.Value;
        }
        
        protected float GetCalculatedSpeedY()
        {
            return moveSpeedY * Config.gameSpeed.Value;
        }
    }
}