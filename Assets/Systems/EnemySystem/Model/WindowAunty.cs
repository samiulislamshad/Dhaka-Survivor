using Systems.AudioSystem.Handler;
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
        [SerializeField] private OneShotPlayer deathSound;
        private bool _alreadyCalledOnce;
        
        [Inject] private SignalBus _signalBus;
        private AddScoreSignal _addScoreSignal;
        private CompositeDisposable _disposable;
        
        public bool isDead;

        private void Start()
        {
            _addScoreSignal = new AddScoreSignal(0);
        }

        public void Spawn()
        {
            isDead = false;
            _alreadyCalledOnce = false;
            animator.Play($"ClosedWindow");
            col.isTrigger = true;
            _disposable = new CompositeDisposable();
            enemyDamageDetector.OnCollisionEnter2d.Subscribe(OnTakeDamage).AddTo(_disposable);
        }

        public void Despawn()
        {
            col.isTrigger = false;
            _disposable?.Dispose();
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
            col.isTrigger = false;
            animator.Play("Death");
            _addScoreSignal.score = scoreValue;
            _signalBus.Fire(_addScoreSignal);
            deathSound.PlayAudio();
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