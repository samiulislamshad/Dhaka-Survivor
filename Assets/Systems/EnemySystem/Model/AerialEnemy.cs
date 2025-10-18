using Systems.EnemySystem.Enum;
using UnityEngine;

namespace Systems.EnemySystem.Model
{
    public class AerialEnemy : Enemy
    {
        public override EnemyType Type => EnemyType.Aerial;

        protected override void OnUpdate()
        {
            var wave = Mathf.Sin(timer * 3f) * 2f;
            transform.Translate(new Vector3(wave * Time.deltaTime, -4f * Time.deltaTime, 0));
        }
    }
}