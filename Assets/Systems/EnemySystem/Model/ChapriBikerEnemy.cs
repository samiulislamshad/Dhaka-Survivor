using Systems.AudioSystem.Handler;
using Systems.EnemySystem.Enum;
using Systems.PlayerSystem.Signals.GameSignals;
using UnityEngine;

namespace Systems.EnemySystem.Model
{
    public class ChapriBikerEnemy : Enemy
    {
        public override EnemyType Type => EnemyType.ChapriBiker;
        private const float BaseEnemySpeed = 2f;

        [SerializeField] private SfxPlayer bikeSfx;
        private Vector2 _initialSfxPos;
        private Vector2 _finalSfxPos;

        protected override void OnFixedUpdate()
        {
            rb.MovePosition(rb.position + Vector2.left * (BaseEnemySpeed * Config.gameSpeed.Value * Time.fixedDeltaTime));
            PlaySfx();
        }
        
        public override string GetEnemyName() => "ChapriBiker";

        private const float Points = 25f;
        private void PlaySfx()
        {
            var pos = transform.position;
            if (!(pos.x <= Points) || !(pos.x >= -Points))
            {
                if(bikeSfx.IsPlaying())
                    bikeSfx.StopSfx();
                return;
            }
            var value = (pos.x + Points) / (Points - Points);
            bikeSfx.PlaySfx();
            bikeSfx.SetVolume(value);
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("Limit"))
            {
                Debug.Log($"Triggered {other.gameObject.name}");
                IsDespawning = true;
            }
            
            if(other.gameObject.CompareTag("Player") && !IsDead)
                SignalBus.Fire<ContactWithEnemySignal>();
        }
    }
}