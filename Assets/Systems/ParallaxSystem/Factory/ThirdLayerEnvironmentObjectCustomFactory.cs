using System;
using Systems.ParallaxSystem.Model;
using UnityEngine;
using Zenject;

namespace Systems.ParallaxSystem.Factory
{
    [Serializable]
    public class ThirdLayerEnvironmentObjectCustomFactory : IFactory<EnvironmentObjectData, Vector3, ThirdLayerEnvironmentObject>
    {
        private readonly DiContainer _container;
    
        public ThirdLayerEnvironmentObjectCustomFactory(DiContainer container)
        {
            _container = container;
        }
    
        public ThirdLayerEnvironmentObject Create(EnvironmentObjectData data, Vector3 position)
        {
            var instance = _container.InstantiatePrefabForComponent<ThirdLayerEnvironmentObject>(data.prefab);
            instance.Initialize(data, position);
            return instance;
        }
    }
}