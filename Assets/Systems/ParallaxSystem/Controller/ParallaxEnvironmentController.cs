using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Systems.GameSystem.Config;
using Systems.ParallaxSystem.Config;
using Systems.ParallaxSystem.Enum;
using Systems.ParallaxSystem.Handler;
using Systems.ParallaxSystem.Model;
using Systems.ParallaxSystem.View;
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
        private ParallaxEnvironmentView _view;

        private GameConfig _gameConfig;
        private ParallaxLayerConfig _config;

        private List<EnvironmentObject> _firstLayerEnvironmentObjects;
        private List<EnvironmentObject> _secondLayerEnvironmentObjects;
        private List<EnvironmentObject> _thirdLayerEnvironmentObjects;
        private List<EnvironmentObject> _fourthLayerEnvironmentObjects;

        // Dictionary to track active environment objects by their GUID
        private Dictionary<string, ActiveEnvironmentObjectData> _activeEnvironmentObjects;

        // Random Spawning variables
        private bool _isRandomSpawning;
        private float _firstLayerRandomSpawnWaitTime = 1.5f;
        private float _secondLayerRandomSpawnWaitTime = 5f;


        public ParallaxEnvironmentController(ParallaxEnvironmentSpawner spawner, GameConfig gameConfig,
            ParallaxLayerConfig config, ParallaxEnvironmentView view)
        {
            _spawner = spawner;
            _gameConfig = gameConfig;
            _config = config;
            _view = view;

            _disposable = new CompositeDisposable();
            _firstLayerEnvironmentObjects = new List<EnvironmentObject>();
            _secondLayerEnvironmentObjects = new List<EnvironmentObject>();
            _thirdLayerEnvironmentObjects = new List<EnvironmentObject>();
            _fourthLayerEnvironmentObjects = new List<EnvironmentObject>();

            _activeEnvironmentObjects = new Dictionary<string, ActiveEnvironmentObjectData>();

            _isRandomSpawning = false;

            StartRandomSpawning(_config.firstParallaxLayer, _firstLayerRandomSpawnWaitTime,
                _view.firstLayerSpawnPoint.transform.position).Forget();
            StartRandomSpawning(_config.secondParallaxLayer, _secondLayerRandomSpawnWaitTime,
                _view.secondLayerSpawnPoint.transform.position).Forget();
            SpawnFirstLayerObjects();
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

                var spawnPosition = CalculateRelativeSpawnPosition(sourceObject, relativeData.distance);
                spawnPosition.y = _view.firstLayerSpawnPoint.transform.position.y;
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
                spawnPosition.y = _view.firstLayerSpawnPoint.transform.position.y;
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
            var spawnPoint = _view.firstLayerSpawnPoint.transform.position;
            return sourceObject.transform.position + new Vector3(distance, spawnPoint.y, spawnPoint.z);
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

        private void SpawnFirstLayerObjects()
        {
            var envData = GetEnvironmentObjectDataById("StartingChunk");
            var envObj = SpawnEnvironmentObject(envData, new Vector3(100, -8));
        }

        #region Random Spawning

        private async UniTaskVoid StartRandomSpawning(ParallaxLayer parallaxLayer, float spawnWaitTime,
            Vector3 position)
        {
            await UniTask.Delay(1000);
            _isRandomSpawning = true;

            while (_isRandomSpawning)
            {
                await SpawnRandomEnvironmentObject(parallaxLayer, spawnWaitTime, position);
            }
        }

        private async UniTask SpawnRandomEnvironmentObject(ParallaxLayer parallaxLayer, float randomSpawnWaitTime,
            Vector3 randomSpawnPosition)
        {
            // Wait if game hasn't started yet
            if (!_gameConfig.hasGameStarted.Value)
            {
                await UniTask.Delay(100);
                return;
            }

            // Get a random environment object from the first layer
            var randomObjectData = GetRandomEnvironmentObjectData(parallaxLayer);
            if (randomObjectData == null)
            {
                await UniTask.Delay(2000);
                return;
            }

            // Spawn at a position off-screen to the right
            var spawnedObject = SpawnEnvironmentObject(randomObjectData, randomSpawnPosition);

            if (spawnedObject == null)
            {
                await UniTask.Delay((int)(randomSpawnWaitTime * 1000));
                return;
            }

            // Check if this object has crucial or preferred relative objects
            var hasRelativeObjects = HasCrucialOrPreferredObjects(randomObjectData);

            if (hasRelativeObjects)
            {
                // Wait a frame to allow the object to be tracked
                await UniTask.Yield();

                // Spawn crucial relative objects immediately
                SpawnCrucialRelativeObjects(spawnedObject, randomObjectData,
                    _activeEnvironmentObjects[spawnedObject.Guid]);

                // Spawn preferred relative objects if conditions are met
                SpawnPreferredRelativeObjects(spawnedObject, randomObjectData,
                    _activeEnvironmentObjects[spawnedObject.Guid]);
            }

            // Wait before spawning the next random object
            // Wait longer if we spawned relatives (to avoid cluttering)
            var waitTime = hasRelativeObjects ? randomSpawnWaitTime * 1.5f : randomSpawnWaitTime;
            await UniTask.Delay((int)(waitTime * 1000));
        }

        /// <summary>
        /// NEWLY ADDED: Helper method to spawn an environment object with proper setup
        /// Centralizes all spawn logic: GUID generation, signal subscription, tracking, etc.
        /// </summary>
        private EnvironmentObject SpawnEnvironmentObject(EnvironmentObjectData objectData, Vector3 position)
        {
            var spawnedObject = _spawner.SpawnById(objectData.id, objectData.layerType, position);

            if (spawnedObject == null) return null;

            // Generate unique GUID for this instance
            var guid = Guid.NewGuid().ToString();
            spawnedObject.Guid = guid;

            // Subscribe to this object's despawn signal
            spawnedObject.OnDespawnSignal
                .Subscribe(OnEnvironmentObjectDespawned)
                .AddTo(_disposable);

            // Create and track active data
            var activeData = new ActiveEnvironmentObjectData(objectData);
            _activeEnvironmentObjects[guid] = activeData;

            // Add to appropriate layer list
            AddToLayerList(spawnedObject, objectData.layerType);
            spawnedObject.gameObject.SetActive(true);

            return spawnedObject;
        }

        /// <summary>
        /// NEWLY ADDED: Gets a random environment object data from the first parallax layer
        /// </summary>
        private EnvironmentObjectData GetRandomEnvironmentObjectData(ParallaxLayer parallaxLayer)
        {
            if (parallaxLayer.environmentObjects == null ||
                parallaxLayer.environmentObjects.Count == 0)
            {
                return null;
            }

            var randomIndex = UnityEngine.Random.Range(0, parallaxLayer.environmentObjects.Count);
            return parallaxLayer.environmentObjects[randomIndex];
        }

        /// <summary>
        /// NEWLY ADDED: Checks if an object has crucial or preferred relative objects
        /// </summary>
        private bool HasCrucialOrPreferredObjects(EnvironmentObjectData objectData)
        {
            bool hasCrucial = objectData.crucialRelativeObjects != null &&
                              objectData.crucialRelativeObjects.Count > 0;
            bool hasPreferred = objectData.preferredRelativeObjects != null &&
                                objectData.preferredRelativeObjects.Count > 0;

            return hasCrucial || hasPreferred;
        }

        /// <summary>
        /// NEWLY ADDED: Public method to stop random spawning
        /// </summary>
        public void StopRandomSpawning()
        {
            _isRandomSpawning = false;
        }

        /// <summary>
        /// NEWLY ADDED: Public method to change random spawn wait time
        /// </summary>
        public void SetRandomSpawnWaitTime(float waitTime)
        {
            _firstLayerRandomSpawnWaitTime = Mathf.Max(0.5f, waitTime); // Minimum 0.5 seconds
        }

        #endregion

        // This method is called when any EnvironmentObject fires its despawn signal
        private void OnEnvironmentObjectDespawned(string guid)
        {
            // Remove from active tracking
            if (_activeEnvironmentObjects.Remove(guid))
                Debug.Log($"Stopped tracking object with GUID: {guid}");
        }

        public void Dispose()
        {
            _disposable?.Dispose();
        }
    }
}