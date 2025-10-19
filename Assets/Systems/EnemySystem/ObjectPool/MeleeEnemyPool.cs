using System;
using Systems.EnemySystem.Model;
using UnityEngine;
using Zenject;

namespace Systems.EnemySystem.ObjectPool
{
    [Serializable]
    public class MeleeEnemyPool : MonoMemoryPool<Vector3, MeleeEnemy>
    {
        protected override void Reinitialize(Vector3 pos, MeleeEnemy enemy)
        {
            enemy.Initialize(pos);
        }

        protected override void OnDespawned(MeleeEnemy enemy)
        {
            enemy.gameObject.SetActive(false);
        }
    }
}