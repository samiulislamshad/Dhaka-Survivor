using System;
using Systems.ParallaxSystem.Model;
using UnityEngine;
using Zenject;

namespace Systems.ParallaxSystem.ObjectPooling
{
    [Serializable]
    public class FirstLayerPool : MonoMemoryPool<Vector3, FirstLayerEnvironmentObject>
    {
        protected override async void Reinitialize(Vector3 pos, FirstLayerEnvironmentObject environmentObject)
        {
            await environmentObject.Initialize(pos);
            environmentObject.OnSpawned();
        }

        protected override void OnDespawned(FirstLayerEnvironmentObject environmentObject)
        {
            environmentObject.OnDespawned();
        }
    }
}