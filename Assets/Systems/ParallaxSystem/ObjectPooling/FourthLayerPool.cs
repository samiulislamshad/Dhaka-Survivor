using System;
using System.Collections.Generic;
using System.Linq;
using Systems.ParallaxSystem.Config;
using Systems.ParallaxSystem.Factory;
using Systems.ParallaxSystem.Model;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace Systems.ParallaxSystem.ObjectPooling
{
    [Serializable]
    public class FourthLayerPool : IDisposable
    {
        private readonly Dictionary<string, Stack<FourthLayerEnvironmentObject>> _objectsByType = new();
        private readonly Dictionary<string, EnvironmentObjectData> _dataByType = new();
        private readonly FourthLayerEnvironmentObjectFactory _factory;
        private readonly ParallaxLayerConfig _config;
        private readonly Transform _parentTransform;

        public FourthLayerPool(
            FourthLayerEnvironmentObjectFactory factory,
            ParallaxLayerConfig config,
            [Inject(Id = "FourthLayerParent")] Transform parentTransform)
        {
            _factory = factory;
            _config = config;
            _parentTransform = parentTransform;

            // Cache data by ID
            foreach (var envObj in _config.fourthParallaxLayer.environmentObjects)
            {
                _dataByType[envObj.id] = envObj;
                _objectsByType[envObj.id] = new Stack<FourthLayerEnvironmentObject>();
            }
        }

        public void InitializePool(int initialCount = 3)
        {
            foreach (var envObj in _config.fourthParallaxLayer.environmentObjects)
            {
                for (var i = 0; i < initialCount; i++)
                {
                    var obj = _factory.Create(envObj, Vector3.zero);
                    obj.transform.SetParent(_parentTransform);
                    obj.gameObject.SetActive(false);
                    _objectsByType[envObj.id].Push(obj);
                }

                Debug.Log($"Initialized {initialCount} instances of {envObj.name} (ID: {envObj.id})");
            }
        }
        
        public FourthLayerEnvironmentObject Spawn(Vector3 position)
        {
            // Get a random ID from available objects
            if (_dataByType.Count == 0)
            {
                Debug.LogError("No objects configured in FourthLayerPool");
                return null;
            }
    
            // Pick random ID
            var randomIndex = Random.Range(0, _dataByType.Count);
            var randomId = _dataByType.Keys.ElementAt(randomIndex);
    
            // Use SpawnById with the random ID
            return SpawnById(randomId, position);
        }

        public FourthLayerEnvironmentObject SpawnById(string id, Vector3 position)
        {
            if (!_objectsByType.ContainsKey(id))
            {
                Debug.LogError($"Object with ID {id} not found in FourthLayerPool");
                return null;
            }

            FourthLayerEnvironmentObject obj;

            if (_objectsByType[id].Count > 0)
            {
                obj = _objectsByType[id].Pop();
                obj.Reinitialize(position);
                obj.OnSpawned();
            }
            else
            {
                // Create new if pool is empty
                obj = _factory.Create(_dataByType[id], position);
                obj.transform.SetParent(_parentTransform);
                obj.OnSpawned();
            }

            return obj;
        }

        public void Despawn(FourthLayerEnvironmentObject obj)
        {
            if (obj == null || string.IsNullOrEmpty(obj.Id))
            {
                Debug.LogWarning("Trying to despawn null or uninitialized object");
                return;
            }

            obj.OnDespawned();

            if (_objectsByType.ContainsKey(obj.Id))
            {
                _objectsByType[obj.Id].Push(obj);
            }
        }

        public void Dispose()
        {
            foreach (var stack in _objectsByType.Values)
            {
                while (stack.Count > 0)
                {
                    var obj = stack.Pop();
                    if (obj != null && obj.gameObject != null)
                    {
                        UnityEngine.Object.Destroy(obj.gameObject);
                    }
                }
            }

            _objectsByType.Clear();
        }
    }
}