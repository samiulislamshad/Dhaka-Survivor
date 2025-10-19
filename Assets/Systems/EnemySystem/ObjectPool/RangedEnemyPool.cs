using System;
using Systems.EnemySystem.Model;
using UnityEngine;
using Zenject;

namespace Systems.EnemySystem.ObjectPool
{
    [Serializable]
    public class RangedEnemyPool : MonoMemoryPool<Vector3, RangedEnemy>
    {
        protected override void Reinitialize(Vector3 pos, RangedEnemy enemy)
        {
            enemy.Initialize(pos);
        }

        protected override void OnDespawned(RangedEnemy enemy)
        {
            enemy.gameObject.SetActive(false);
        }
    }
}