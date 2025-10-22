using Systems.ParallaxSystem.Model;
using UnityEngine;
using Zenject;

namespace Systems.ParallaxSystem.Factory
{
    public class FirstLayerEnvironmentObjectCustomFactory : IFactory<EnvironmentObjectData, Vector3, FirstLayerEnvironmentObject>
    {
        private readonly DiContainer _container;
    
        public FirstLayerEnvironmentObjectCustomFactory(DiContainer container)
        {
            _container = container;
        }
    
        public FirstLayerEnvironmentObject Create(EnvironmentObjectData data, Vector3 position)
        {
            var instance = _container.InstantiatePrefabForComponent<FirstLayerEnvironmentObject>(data.prefab);
            instance.Initialize(data, position);
            return instance;
        }
    }
}