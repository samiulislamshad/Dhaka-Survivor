using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Systems.GameSystem.Config;
using Systems.ParallaxSystem.Config;
using Systems.ParallaxSystem.Enum;
using Systems.ParallaxSystem.Handler;
using Systems.ParallaxSystem.Model;
using UniRx;
using UnityEngine;
using Zenject;

namespace Systems.ParallaxSystem.Controller
{
    [Serializable]
    public class ParallaxEnvironmentController : IFixedTickable, IDisposable
    {
        private ParallaxEnvironmentSpawner _spawner;
        private CompositeDisposable _disposable;

        private GameConfig _gameConfig;
        private ParallaxLayerConfig _config;

        private List<EnvironmentObject> _firstLayerEnvironmentObjects;
        private List<EnvironmentObject> _secondLayerEnvironmentObjects;
        private List<EnvironmentObject> _thirdLayerEnvironmentObjects;
        private List<EnvironmentObject> _fourthLayerEnvironmentObjects;

        // Dictionary to track active environment objects by their GUID
        private Dictionary<string, ActiveEnvironmentObjectData> _activeEnvironmentObjects;
        
        public ParallaxEnvironmentController(ParallaxEnvironmentSpawner spawner, GameConfig gameConfig,
            ParallaxLayerConfig config)
        {
            _spawner = spawner;
            _gameConfig = gameConfig;
            _config = config;

            _disposable = new CompositeDisposable();
            _firstLayerEnvironmentObjects = new List<EnvironmentObject>();
            _secondLayerEnvironmentObjects = new List<EnvironmentObject>();
            _thirdLayerEnvironmentObjects = new List<EnvironmentObject>();
            _fourthLayerEnvironmentObjects = new List<EnvironmentObject>();
            
            _activeEnvironmentObjects = new Dictionary<string, ActiveEnvironmentObjectData>();
            
            SpawnFirstLayerObjects().Forget();
        }

        public void FixedTick()
        {
            if (!_gameConfig.hasGameStarted.Value) return;
            UpdateEnvironmentObjects();
            UpdateActiveObjectTimers();
            CheckAndSpawnRelativeObjects();
        }

        private void UpdateEnvironmentObjects()
        {
            var gameSpeed = _gameConfig.gameSpeed.Value;
        
            UpdateLayerObjects(_firstLayerEnvironmentObjects, gameSpeed);
            UpdateLayerObjects(_secondLayerEnvironmentObjects, gameSpeed);
            UpdateLayerObjects(_thirdLayerEnvironmentObjects, gameSpeed);
            UpdateLayerObjects(_fourthLayerEnvironmentObjects, gameSpeed);
        }
        
        private void UpdateLayerObjects(List<EnvironmentObject> objects, float gameSpeed)
        {
            for (var i = objects.Count - 1; i >= 0; i--)
            {
                var obj = objects[i];
                obj.OnFixedUpdate(gameSpeed);
            
                // Check if object should despawn
                if (obj.ShouldDespawn())
                {
                    objects.RemoveAt(i);
                }
            }
        }
        
        private void UpdateActiveObjectTimers()
        {
            var deltaTime = Time.fixedDeltaTime;
            foreach (var kvp in _activeEnvironmentObjects.ToList())
            {
                kvp.Value.timeElapsedSinceSpawn += deltaTime;
            }
        }
        
        private void CheckAndSpawnRelativeObjects()
        {
            // Check all spawned objects for relative spawning opportunities
            var objectsToCheck = new List<EnvironmentObject>(_firstLayerEnvironmentObjects);
        
            foreach (var envObj in objectsToCheck)
            {
                if (!_activeEnvironmentObjects.TryGetValue(envObj.Guid, out var activeData))
                    continue;

                var objectData = GetEnvironmentObjectDataById(envObj.Id);
                if (objectData == null) continue;

                // Spawn crucial relative objects
                SpawnCrucialRelativeObjects(envObj, objectData, activeData);
            
                // Spawn preferred relative objects (if conditions are met)
                SpawnPreferredRelativeObjects(envObj, objectData, activeData);
            }
        }
        
        private void SpawnCrucialRelativeObjects(EnvironmentObject sourceObject, 
            EnvironmentObjectData objectData, ActiveEnvironmentObjectData activeData)
        {
            if (objectData.crucialRelativeObjects == null || objectData.crucialRelativeObjects.Count == 0)
                return;

            foreach (var relativeData in objectData.crucialRelativeObjects)
            {
                // Check if this crucial object has already been spawned for this source object
                string spawnKey = $"{sourceObject.Guid}_crucial_{relativeData.relativeObjectId}";
            
                if (HasAlreadySpawnedRelative(spawnKey))
                    continue;

                // Spawn the crucial relative object
                var relativeObjectData = GetEnvironmentObjectDataById(relativeData.relativeObjectId);
                if (relativeObjectData == null) continue;

                Vector3 spawnPosition = CalculateRelativeSpawnPosition(sourceObject, relativeData.distance);
                SpawnRelativeObject(relativeObjectData, spawnPosition, spawnKey);
            }
        }
        
        private void SpawnPreferredRelativeObjects(EnvironmentObject sourceObject, 
            EnvironmentObjectData objectData, ActiveEnvironmentObjectData activeData)
        {
            if (objectData.preferredRelativeObjects == null || objectData.preferredRelativeObjects.Count == 0)
                return;

            foreach (var relativeData in objectData.preferredRelativeObjects)
            {
                // Check if this preferred object has already been spawned for this source object
                string spawnKey = $"{sourceObject.Guid}_preferred_{relativeData.relativeObjectId}";
            
                if (HasAlreadySpawnedRelative(spawnKey))
                    continue;

                // Check if an instance of this object type is currently active and within spawn time
                if (IsObjectTypeRecentlyActive(relativeData.relativeObjectId))
                    continue;

                // Spawn the preferred relative object
                var relativeObjectData = GetEnvironmentObjectDataById(relativeData.relativeObjectId);
                if (relativeObjectData == null) continue;

                Vector3 spawnPosition = CalculateRelativeSpawnPosition(sourceObject, relativeData.distance);
                SpawnRelativeObject(relativeObjectData, spawnPosition, spawnKey);
            }
        }
        
        private bool HasAlreadySpawnedRelative(string spawnKey)
        {
            return _activeEnvironmentObjects.ContainsKey(spawnKey);
        }
        
        private bool IsObjectTypeRecentlyActive(string objectId)
        {
            foreach (var kvp in _activeEnvironmentObjects)
            {
                var activeData = kvp.Value;
            
                // Check if this is the same type of object
                if (activeData.id == objectId)
                {
                    // Check if it's still within its estimated spawn time (recently active)
                    if (activeData.timeElapsedSinceSpawn < activeData.estimatedSpawnTime)
                    {
                        return true;
                    }
                }
            }
        
            return false;
        }
        
        private Vector3 CalculateRelativeSpawnPosition(EnvironmentObject sourceObject, float distance)
        {
            // Spawn to the right of the source object at the specified distance
            return sourceObject.transform.position + new Vector3(distance, 0, 0);
        }
        
        private void SpawnRelativeObject(EnvironmentObjectData objectData, Vector3 position, string trackingKey)
        {
            var spawnedObject = _spawner.SpawnById(objectData.id, objectData.layerType, position);
        
            if (spawnedObject == null) return;

            // Generate a GUID for tracking, but use the tracking key for relative objects
            spawnedObject.Guid = trackingKey;
            spawnedObject.OnDespawnSignal.Subscribe(OnEnvironmentObjectDespawned).AddTo(_disposable);
        
            // Create active data and track it
            var activeData = new ActiveEnvironmentObjectData(objectData);
            _activeEnvironmentObjects[trackingKey] = activeData;
        
            // Add to appropriate layer list
            AddToLayerList(spawnedObject, objectData.layerType);
        
            spawnedObject.gameObject.SetActive(true);
        }
        
        private void AddToLayerList(EnvironmentObject obj, EnvironmentLayerType layerType)
        {
            switch (layerType)
            {
                case EnvironmentLayerType.First:
                    if (!_firstLayerEnvironmentObjects.Contains(obj))
                        _firstLayerEnvironmentObjects.Add(obj);
                    break;
                case EnvironmentLayerType.Second:
                    if (!_secondLayerEnvironmentObjects.Contains(obj))
                        _secondLayerEnvironmentObjects.Add(obj);
                    break;
                case EnvironmentLayerType.Third:
                    if (!_thirdLayerEnvironmentObjects.Contains(obj))
                        _thirdLayerEnvironmentObjects.Add(obj);
                    break;
                case EnvironmentLayerType.Fourth:
                    if (!_fourthLayerEnvironmentObjects.Contains(obj))
                        _fourthLayerEnvironmentObjects.Add(obj);
                    break;
            }
        }
        
        private EnvironmentObjectData GetEnvironmentObjectDataById(string id)
        {
            // Search in first layer
            var obj = _config.firstParallaxLayer.environmentObjects?.FirstOrDefault(x => x.id == id);
            if (obj != null) return obj;
        
            // Search in other layers if needed
            obj = _config.secondParallaxLayer?.environmentObjects?.FirstOrDefault(x => x.id == id);
            if (obj != null) return obj;
        
            obj = _config.thirdParallaxLayer?.environmentObjects?.FirstOrDefault(x => x.id == id);
            if (obj != null) return obj;
        
            obj = _config.fourthParallaxLayer?.environmentObjects?.FirstOrDefault(x => x.id == id);
            return obj;
        }

        private async UniTaskVoid SpawnFirstLayerObjects()
        {
            await UniTask.Delay(2000);

            foreach (var envObj in _config.firstParallaxLayer.environmentObjects)
            {
                for (var i = 0; i < 3; i++)
                {
                    var spawnPosition = new Vector3(UnityEngine.Random.Range(0, 5), UnityEngine.Random.Range(0, 5), 0);
                    var firstLayerObject = _spawner.SpawnById(envObj.id, envObj.layerType, spawnPosition);
                    firstLayerObject.OnDespawnSignal.Subscribe(OnEnvironmentObjectDespawned).AddTo(_disposable);
                
                    if (firstLayerObject == null) continue;
                
                    // Generate unique GUID for this instance
                    string guid = Guid.NewGuid().ToString();
                    firstLayerObject.Guid = guid;
                
                    // Create and track active data
                    var activeData = new ActiveEnvironmentObjectData(envObj);
                    _activeEnvironmentObjects[guid] = activeData;
                
                    if (_firstLayerEnvironmentObjects.Contains(firstLayerObject)) continue;
                    _firstLayerEnvironmentObjects.Add(firstLayerObject);
                    firstLayerObject.gameObject.SetActive(true);
                }
            }
        }
        
        // private void HandleObjectDespawn(EnvironmentObject obj)
        // {
        //     // Notify that this object is despawning
        //     OnObjectDespawned.OnNext(obj.Guid);
        // }
        
        // This method is called when any EnvironmentObject fires its despawn signal
        private void OnEnvironmentObjectDespawned(string guid)
        {
            // Remove from active tracking
            if (_activeEnvironmentObjects.Remove(guid))
                Debug.Log($"Stopped tracking object with GUID: {guid}");
        }
        
        // Public method for EnvironmentObject to call when despawning
        // public IObservable<string> OnObjectDespawned => _onObjectDespawned;

        public void Dispose()
        {
            _disposable?.Dispose();
        }
    }
}