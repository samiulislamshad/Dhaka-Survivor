using System;
using Systems.ParallaxSystem.Model;
using UnityEngine;
using Zenject;

namespace Systems.ParallaxSystem.ObjectPooling
{
    [Serializable]
    public class FourthLayerPool : MonoMemoryPool<Vector3, FourthLayerEnvironmentObject>
    {
        protected override async void Reinitialize(Vector3 pos, FourthLayerEnvironmentObject environmentObject)
        {
            await environmentObject.Initialize(pos);
            environmentObject.OnSpawned();
        }

        protected override void OnDespawned(FourthLayerEnvironmentObject environmentObject)
        {
            environmentObject.OnDespawned();
        }
    }
}