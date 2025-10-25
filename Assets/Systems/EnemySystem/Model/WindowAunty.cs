using System;
using Systems.EnemySystem.Service;
using Systems.PlayerSystem.Signals.GameSignals;
using Systems.ScoreSystem.Signal;
using UniRx;
using UnityEngine;
using Zenject;

namespace Systems.EnemySystem.Model
{
    public class WindowAunty : MonoBehaviour
    {
        [SerializeField] private Collider2D col;
        [SerializeField] private Animator animator;
        [SerializeField] private EnemyDamageDetector enemyDamageDetector;
        
        [SerializeField] private int scoreValue;
        private bool _alreadyCalledOnce;
        
        [Inject] private SignalBus _signalBus;
        private AddScoreSignal _addScoreSignal;
        private IDisposable _disposable;
        
        public bool isDead;

        private void Start()
        {
            _addScoreSignal = new AddScoreSignal(0);
            _disposable = new CompositeDisposable();
        }

        public void Initialize()
        {
            isDead = false;
            _alreadyCalledOnce = false;
            animator.Play($"ClosedWindow");
            col.includeLayers = LayerMask.GetMask("Player");
            _disposable = enemyDamageDetector.OnCollisionEnter2d.Subscribe(OnTakeDamage);
        }

        public void OnUpdate()
        {
            if(_alreadyCalledOnce) return;
            _alreadyCalledOnce = true;
            
            animator.Play($"OpenWindow");
            col.includeLayers = LayerMask.GetMask("Player");
        }
        
        private void OnTakeDamage(Collision2D collision)
        {
            isDead = true;
            animator.Play("Death");
            col.includeLayers = LayerMask.GetMask();
            _addScoreSignal.score = scoreValue;
            _signalBus.Fire(_addScoreSignal);
            _disposable.Dispose();
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if(isDead) return;
            if(!other.gameObject.CompareTag("Player")) return;
            _signalBus.Fire<ContactWithEnemySignal>();
        }

        private void OnDestroy()
        {
            _disposable?.Dispose();
        }
    }
}