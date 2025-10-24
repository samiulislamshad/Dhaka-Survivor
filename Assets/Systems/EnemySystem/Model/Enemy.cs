using System;
using Systems.EnemySystem.Enum;
using Systems.EnemySystem.Interface;
using Systems.EnemySystem.Service;
using Systems.GameSystem.Config;
using Systems.PlayerSystem.Signals;
using UniRx;
using UnityEngine;
using Zenject;

namespace Systems.EnemySystem.Model
{
    public abstract class Enemy : MonoBehaviour, IEnemy, IDisposable
    {
        [Inject] protected GameConfig Config;
        [Inject] protected SignalBus SignalBus;
        
        protected CompositeDisposable disposable = new();

        public abstract EnemyType Type { get; }
        public Animator animator;
        
        [SerializeField] protected Rigidbody2D rb;
        [SerializeField] private Collider2D col;
        [SerializeField] protected EnemyDamageDetector enemyDamageDetector;
        
        protected bool IsDespawning;

        [Header("Movement Settings")]
        [SerializeField] protected float moveSpeedX = 1f;
        [SerializeField] protected float moveSpeedY;

        public virtual void Initialize(Vector3 position)
        {
            disposable = new CompositeDisposable();
            transform.position = position;
            
            IsDespawning = false;
            gameObject.SetActive(true);
            col.includeLayers = LayerMask.GetMask("Player");
            
            animator.Play("Idle");

            if (enemyDamageDetector != null)
            {
                enemyDamageDetector.gameObject.SetActive(true);
                enemyDamageDetector.OnCollisionEnter2d.Subscribe(OnTakeDamage).AddTo(disposable);
            }
        }

        private void FixedUpdate()
        {
            OnFixedUpdate();
        }

        protected abstract void OnFixedUpdate();

        public virtual bool ShouldDespawn()
        {
            return IsDespawning;
        }

        private void OnTakeDamage(Collision2D collision)
        {
            col.excludeLayers = LayerMask.GetMask("Player");
            animator.Play("Death");
            SignalBus.Fire<PlayerSpecialJumpSignal>();
        }
        
        protected float GetCalculatedSpeedX()
        {
            return moveSpeedX * Config.gameSpeed.Value;
        }
        
        protected float GetCalculatedSpeedY()
        {
            return moveSpeedY * Config.gameSpeed.Value;
        }

        public void Dispose()
        {
            disposable.Dispose();
        }

        public virtual void OnDestroy()
        {
            Dispose();
        }
    }
}