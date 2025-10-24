using System;
using Systems.EnemySystem.Model;
using UnityEngine;
using Zenject;

namespace Systems.EnemySystem.ObjectPool
{
    [Serializable]
    public class Chesra1Pool : MonoMemoryPool<Vector3, Chesra1Enemy>
    {
        protected override void Reinitialize(Vector3 pos, Chesra1Enemy enemy)
        {
            enemy.Initialize(pos);
        }

        protected override void OnDespawned(Chesra1Enemy enemy)
        {
            enemy.gameObject.SetActive(false);
        }
    }
}