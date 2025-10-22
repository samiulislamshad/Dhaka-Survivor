using Systems.ParallaxSystem.Model;
using UnityEngine;
using Zenject;

namespace Systems.ParallaxSystem.Factory
{
    public class SecondLayerEnvironmentObjectCustomFactory : IFactory<EnvironmentObjectData, Vector3, SecondLayerEnvironmentObject>
    {
        private readonly DiContainer _container;
    
        public SecondLayerEnvironmentObjectCustomFactory(DiContainer container)
        {
            _container = container;
        }
    
        public SecondLayerEnvironmentObject Create(EnvironmentObjectData data, Vector3 position)
        {
            var instance = _container.InstantiatePrefabForComponent<SecondLayerEnvironmentObject>(data.prefab);
            instance.Initialize(data, position);
            return instance;
        }
    }
}