using System;
using Systems.EnemySystem.Model;
using UnityEngine;
using Zenject;

namespace Systems.EnemySystem.ObjectPool
{
    [Serializable]
    public class PagriBroPool : MonoMemoryPool<Vector3, PagriBroEnemy>
    {
        protected override void Reinitialize(Vector3 pos, PagriBroEnemy enemy)
        {
            enemy.Initialize(pos);
        }

        protected override void OnDespawned(PagriBroEnemy enemy)
        {
            enemy.gameObject.SetActive(false);
        }
    }
}