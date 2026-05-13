using System;
using Systems.AudioSystem.Handler;
using Systems.EnemySystem.Enum;
using Systems.EnemySystem.Interface;
using Systems.EnemySystem.Service;
using Systems.GameSystem.Config;
using Systems.PlayerSystem.Signals.GameSignals;
using Systems.ScoreSystem.Signal;
using UniRx;
using UnityEngine;
using Zenject;

namespace Systems.EnemySystem.Model
{
    public abstract class Enemy : MonoBehaviour, IEnemy, IDisposable
    {
        [Inject] protected GameConfig Config;
        [Inject] protected SignalBus SignalBus;
        
        private CompositeDisposable _disposable = new();

        public abstract EnemyType Type { get; } 
        public Animator animator;
        [SerializeField] protected Animator speechBubbleAnimator;
        
        [SerializeField] protected Rigidbody2D rb;
        [SerializeField] private Collider2D col;
        [SerializeField] protected EnemyDamageDetector enemyDamageDetector;
        
        [SerializeField] private OneShotPlayer deathOneShotPlayer;

        protected bool IsDead;
        protected bool IsDespawning;
        private const int Score = 20;
        private AddScoreSignal _scoreSignal;

        public bool canShowSpeechBubble;
        protected bool HasShownSpeechBubble;
        protected int SpeechBubbleOffTime = 2000;

        [Header("Movement Settings")]
        [SerializeField] protected float moveSpeedX = 1f;
        [SerializeField] protected float moveSpeedY;

        public virtual void Initialize(Vector3 position)
        {
            _disposable = new CompositeDisposable();
            transform.position = position;
            _scoreSignal = new AddScoreSignal(Score);

            IsDead = false;
            IsDespawning = false;
            gameObject.SetActive(true);
            col.includeLayers = LayerMask.GetMask("Player", "Limit");
            col.excludeLayers = LayerMask.GetMask();
            
            animator.Play("Idle");

            if (enemyDamageDetector != null)
            {
                enemyDamageDetector.gameObject.SetActive(true);
                enemyDamageDetector.OnCollisionEnter2d.Subscribe(OnTakeDamage).AddTo(_disposable);
            }
        }

        private void Start()
        {
            OnStart();
        }

        private void FixedUpdate()
        {
            OnFixedUpdate();
        }

        protected abstract void OnStart();
        protected abstract void OnFixedUpdate();
        public abstract string GetEnemyName();

        public virtual bool ShouldDespawn()
        {
            return IsDespawning;
        }

        private void OnTakeDamage(Collision2D collision)
        {
            IsDead = true;
            col.excludeLayers = LayerMask.GetMask("Player");
            col.includeLayers = LayerMask.GetMask("Limit");
            animator.Play("Death");
            SignalBus.Fire<PlayerSpecialJumpSignal>();
            _scoreSignal = new AddScoreSignal(Score);
            SignalBus.Fire(_scoreSignal);
            deathOneShotPlayer.PlayAudio();
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
            _disposable.Dispose();
        }

        public virtual void OnDestroy()
        {
            Dispose();
        }
    }
}