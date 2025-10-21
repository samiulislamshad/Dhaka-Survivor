using System;
using Systems.ParallaxSystem.Model;
using UnityEngine;
using Zenject;

namespace Systems.ParallaxSystem.ObjectPooling
{
    [Serializable]
    public class SecondLayerPool : MonoMemoryPool<Vector3, SecondLayerEnvironmentObject>
    {
        protected override async void Reinitialize(Vector3 pos, SecondLayerEnvironmentObject environmentObject)
        {
            await environmentObject.Initialize(pos);
            environmentObject.OnSpawned();
        }

        protected override void OnDespawned(SecondLayerEnvironmentObject environmentObject)
        {
            environmentObject.OnDespawned();
        }
    }
}