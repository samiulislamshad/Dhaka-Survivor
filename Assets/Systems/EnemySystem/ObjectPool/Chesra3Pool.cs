using System;
using Systems.EnemySystem.Model;
using UnityEngine;
using Zenject;

namespace Systems.EnemySystem.ObjectPool
{
    [Serializable]
    public class Chesra3Pool : MonoMemoryPool<Vector3, Chesra3Enemy>
    {
        protected override void Reinitialize(Vector3 pos, Chesra3Enemy enemy)
        {
            enemy.Initialize(pos);
        }

        protected override void OnDespawned(Chesra3Enemy enemy)
        {
            enemy.gameObject.SetActive(false);
        }
    }
}