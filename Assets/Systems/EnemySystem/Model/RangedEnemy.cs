using Systems.EnemySystem.Enum;
using UnityEngine;

namespace Systems.EnemySystem.Model
{
    public class RangedEnemy : Enemy
    {
        public override EnemyType Type => EnemyType.Ranged;

        protected override void OnUpdate()
        {
            transform.Translate(Vector3.down * (3f * Time.deltaTime));
        }
    }
}