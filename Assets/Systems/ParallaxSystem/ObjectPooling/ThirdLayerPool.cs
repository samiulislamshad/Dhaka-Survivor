using System;
using System.Collections.Generic;
using Systems.ParallaxSystem.Config;
using Systems.ParallaxSystem.Model;
using UnityEngine;
using Zenject;

namespace Systems.ParallaxSystem.ObjectPooling
{
    [Serializable]
    public class ThirdLayerPool : MonoMemoryPool<Vector3, ThirdLayerEnvironmentObject>
    {
        private Dictionary<string, Queue<ThirdLayerEnvironmentObject>> _objectsByType = new();
        private ParallaxLayerConfig _config;
        
        [Inject]
        public void Construct(ParallaxLayerConfig config)
        {
            _config = config;
        }

        public void InitializePool(int initialCount = 3)
        {
            foreach (var envObj in _config.thirdParallaxLayer.environmentObjects)
            {
                if (!_objectsByType.ContainsKey(envObj.id))
                {
                    _objectsByType[envObj.id] = new Queue<ThirdLayerEnvironmentObject>();
                }

                // Pre-spawn objects
                for (var i = 0; i < initialCount; i++)
                {
                    var obj = Spawn(Vector3.zero);
                    obj.Initialize(envObj, Vector3.zero);
                    obj.gameObject.SetActive(false);
                    _objectsByType[envObj.id].Enqueue(obj);
                }
            }
        }

        public ThirdLayerEnvironmentObject SpawnById(string id, Vector3 position)
        {
            if (!_objectsByType.ContainsKey(id))
            {
                Debug.LogError($"Object with ID {id} not found in FirstLayerPool");
                return null;
            }

            ThirdLayerEnvironmentObject obj;

            if (_objectsByType[id].Count > 0)
            {
                obj = _objectsByType[id].Dequeue();
                obj.transform.position = position;
                obj.gameObject.SetActive(true);
            }
            else
            {
                // Create new if pool is empty
                obj = Spawn(position);
                var envData = _config.thirdParallaxLayer.environmentObjects.Find(x => x.id == id);
                obj.Initialize(envData, Vector3.zero);
            }

            return obj;
        }
        
        protected override  void Reinitialize(Vector3 pos, ThirdLayerEnvironmentObject environmentObject)
        {
            environmentObject.Reinitialize(pos);
            environmentObject.OnSpawned();
        }

        protected override void OnDespawned(ThirdLayerEnvironmentObject environmentObject)
        {
            environmentObject.OnDespawned();
        }
    }
}