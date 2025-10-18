using System;
using Systems.EnemySystem.Enum;
using UnityEngine;

namespace Systems.EnemySystem.Model
{
    public class MeleeEnemy : Enemy
    {
        public override EnemyType Type => EnemyType.Melee;

        protected override void OnFixedUpdate()
        {
            rb.MovePosition(rb.position + Vector2.left * (Config.movementSpeed.Value * Time.fixedDeltaTime));
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