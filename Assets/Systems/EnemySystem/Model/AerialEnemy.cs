using Systems.EnemySystem.Enum;
using UnityEngine;

namespace Systems.EnemySystem.Model
{
    public class AerialEnemy : Enemy
    {
        public override EnemyType Type => EnemyType.Aerial;

        protected override void OnFixedUpdate()
        {
            var wave = Mathf.Sin(Timer * 3f) * 2f;
            var movement = new Vector2(-Config.gameSpeed.Value * Time.fixedDeltaTime, wave * Time.fixedDeltaTime);
            rb.MovePosition(rb.position + movement);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("Limit"))
            {
                Debug.Log($"Triggered {other.gameObject.name}");
                IsDespawning = true;
            }
        }
        
    }
}