using System;
using Systems.EnemySystem.Model;

namespace Systems.EnemySystem.Signals
{
    [Serializable]
    public class UnregisterEnemySignal
    {
        public Enemy enemy;

        public UnregisterEnemySignal(Enemy enemy)
        {
            this.enemy = enemy;
        }
    }
}