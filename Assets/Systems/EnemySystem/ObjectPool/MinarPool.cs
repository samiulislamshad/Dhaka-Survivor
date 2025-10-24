using System;
using Systems.EnemySystem.Model;
using UnityEngine;
using Zenject;

namespace Systems.EnemySystem.ObjectPool
{
    [Serializable]
    public class MinarPool : MonoMemoryPool<Vector3, MinarEnemy>
    {
        protected override void Reinitialize(Vector3 pos, MinarEnemy enemy)
        {
            enemy.Initialize(pos);
        }

        protected override void OnDespawned(MinarEnemy enemy)
        {
            enemy.gameObject.SetActive(false);
        }
    }
}