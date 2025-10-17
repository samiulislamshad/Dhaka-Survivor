using System;
using Systems.EnemySystem.Enum;

namespace Systems.EnemySystem.Model
{
    [Serializable]
    public class FlyingEnemy : Enemy
    {
        public override EnemyType Type { get; }
    }
}