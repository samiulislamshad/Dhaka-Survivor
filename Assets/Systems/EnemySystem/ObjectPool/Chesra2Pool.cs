using System;
using Systems.EnemySystem.Model;
using UnityEngine;
using Zenject;

namespace Systems.EnemySystem.ObjectPool
{
    [Serializable]
    public class Chesra2Pool : MonoMemoryPool<Vector3, Chesra2Enemy>
    {
        protected override void Reinitialize(Vector3 pos, Chesra2Enemy enemy)
        {
            enemy.Initialize(pos);
        }

        protected override void OnDespawned(Chesra2Enemy enemy)
        {
            enemy.gameObject.SetActive(false);
        }
    }
}