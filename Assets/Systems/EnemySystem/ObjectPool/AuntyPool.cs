using System;
using Systems.EnemySystem.Model;
using UnityEngine;
using Zenject;

namespace Systems.EnemySystem.ObjectPool
{
    [Serializable]
    public class AuntyPool : MonoMemoryPool<Vector3, AuntyEnemy>
    {
        protected override void Reinitialize(Vector3 pos, AuntyEnemy enemy)
        {
            enemy.Initialize(pos);
        }

        protected override void OnDespawned(AuntyEnemy enemy)
        {
            enemy.gameObject.SetActive(false);
        }
    }
}