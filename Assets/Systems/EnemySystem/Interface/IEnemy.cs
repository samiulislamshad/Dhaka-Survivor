using Systems.EnemySystem.Enum;
using UnityEngine;

namespace Systems.EnemySystem.Interface
{
    public interface IEnemy
    {
        EnemyType Type { get; }
        void Initialize(Vector3 position);
        bool ShouldDespawn();
    }
}