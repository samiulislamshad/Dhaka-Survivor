using Systems.EnemySystem.Enum;
using UnityEngine;

namespace Systems.EnemySystem.Model
{
    public class MeleeEnemy : Enemy
    {
        public override EnemyType Type => EnemyType.Melee;

        protected override void OnUpdate()
        {
            transform.Translate(Vector3.down * (5f * Time.deltaTime));
        }
    }
}