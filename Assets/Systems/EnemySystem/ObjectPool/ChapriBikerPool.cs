using System;
using Systems.EnemySystem.Model;
using UnityEngine;
using Zenject;

namespace Systems.EnemySystem.ObjectPool
{
    [Serializable]
    public class ChapriBikerPool : MonoMemoryPool<Vector3, ChapriBikerEnemy>
    {
        protected override void Reinitialize(Vector3 pos, ChapriBikerEnemy enemy)
        {
            enemy.Initialize(pos);
        }

        protected override void OnDespawned(ChapriBikerEnemy enemy)
        {
            enemy.gameObject.SetActive(false);
        }
    }
}