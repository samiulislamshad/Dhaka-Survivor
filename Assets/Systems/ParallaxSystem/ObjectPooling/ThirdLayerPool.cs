using System;
using Systems.ParallaxSystem.Model;
using UnityEngine;
using Zenject;

namespace Systems.ParallaxSystem.ObjectPooling
{
    [Serializable]
    public class ThirdLayerPool : MonoMemoryPool<Vector3, ThirdLayerEnvironmentObject>
    {
        protected override async void Reinitialize(Vector3 pos, ThirdLayerEnvironmentObject environmentObject)
        {
            await environmentObject.Initialize(pos);
            environmentObject.OnSpawned();
        }

        protected override void OnDespawned(ThirdLayerEnvironmentObject environmentObject)
        {
            environmentObject.OnDespawned();
        }
    }
}