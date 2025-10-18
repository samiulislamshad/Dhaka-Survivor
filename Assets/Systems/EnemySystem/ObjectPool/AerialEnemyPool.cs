using Systems.EnemySystem.Model;
using UnityEngine;
using Zenject;

namespace Systems.EnemySystem.ObjectPool
{
    public class AerialEnemyPool : MonoMemoryPool<Vector3, AerialEnemy>
    {
        protected override void Reinitialize(Vector3 pos, AerialEnemy enemy)
        {
            enemy.Initialize(pos);
        }

        protected override void OnDespawned(AerialEnemy enemy)
        {
            enemy.gameObject.SetActive(false);
        }
    }
}