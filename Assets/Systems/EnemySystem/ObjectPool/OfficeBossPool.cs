using System;
using Systems.EnemySystem.Model;
using UnityEngine;
using Zenject;

namespace Systems.EnemySystem.ObjectPool
{
    [Serializable]
    public class OfficeBossPool : MonoMemoryPool<Vector3, OfficeBossEnemy>
    {
        protected override void Reinitialize(Vector3 pos, OfficeBossEnemy enemy)
        {
            enemy.Initialize(pos);
        }

        protected override void OnDespawned(OfficeBossEnemy enemy)
        {
            enemy.gameObject.SetActive(false);
        }
    }
}