using Systems.EnemySystem.Enum;
using Systems.PlayerSystem.Signals.GameSignals;
using UnityEngine;

namespace Systems.EnemySystem.Model
{
    public class ChapriBikerEnemy : Enemy
    {
        public override EnemyType Type => EnemyType.ChapriBiker;
        private const float BaseEnemySpeed = 2f;

        protected override void OnFixedUpdate()
        {
            rb.MovePosition(rb.position + Vector2.left * (BaseEnemySpeed * Config.gameSpeed.Value * Time.fixedDeltaTime));
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