using System;
using Systems.ParallaxSystem.Model;
using UnityEngine;
using Zenject;

namespace Systems.ParallaxSystem.Factory
{
    [Serializable]
    public class FourthLayerEnvironmentObjectCustomFactory : IFactory<EnvironmentObjectData, Vector3, FourthLayerEnvironmentObject>
    {
        private readonly DiContainer _container;
    
        public FourthLayerEnvironmentObjectCustomFactory(DiContainer container)
        {
            _container = container;
        }
    
        public FourthLayerEnvironmentObject Create(EnvironmentObjectData data, Vector3 position)
        {
            var instance = _container.InstantiatePrefabForComponent<FourthLayerEnvironmentObject>(data.prefab);
            instance.Initialize(data, position);
            return instance;
        }
    }
}