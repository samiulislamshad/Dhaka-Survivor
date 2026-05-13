using Systems.EnemySystem.Enum;
using Systems.PlayerSystem.Signals.GameSignals;
using UnityEngine;

namespace Systems.EnemySystem.Model
{
    public class MinarEnemy : Enemy
    {
        public override EnemyType Type => EnemyType.Minar;

        protected override void OnStart()
        {
            
        }

        protected override void OnFixedUpdate()
        {
            var finalSpeedX = GetCalculatedSpeedX();
            var finalSpeedY = GetCalculatedSpeedY();
        
            var movement = new Vector2(-finalSpeedX, finalSpeedY) * (Time.fixedDeltaTime * Config.enemySpeedMultiplier);
            rb.MovePosition(rb.position + movement);
        }
        
        public override string GetEnemyName() => "Minar";
        
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