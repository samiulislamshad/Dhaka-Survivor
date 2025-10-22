using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Systems.GameSystem.Config;
using Systems.ParallaxSystem.Config;
using Systems.ParallaxSystem.Handler;
using Systems.ParallaxSystem.Model;
using UniRx;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

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
            
            SpawnFirstLayerObjects().Forget();
        }

        public void FixedTick()
        {
            if (!_gameConfig.hasGameStarted.Value) return;
            UpdateEnvironmentObjects();
        }

        private void UpdateEnvironmentObjects()
        {
            var gameSpeed = _gameConfig.gameSpeed.Value;
            if (_firstLayerEnvironmentObjects.Count > 0)
                foreach (var environmentObject in _firstLayerEnvironmentObjects)
                    environmentObject.OnFixedUpdate(gameSpeed);
            if (_secondLayerEnvironmentObjects.Count > 0)
                foreach (var environmentObject in _secondLayerEnvironmentObjects)
                    environmentObject.OnFixedUpdate(gameSpeed);
            if (_thirdLayerEnvironmentObjects.Count > 0)
                foreach (var environmentObject in _thirdLayerEnvironmentObjects)
                    environmentObject.OnFixedUpdate(gameSpeed);
            if (_fourthLayerEnvironmentObjects.Count > 0)
                foreach (var environmentObject in _fourthLayerEnvironmentObjects)
                    environmentObject.OnFixedUpdate(gameSpeed);
        }

        private async UniTaskVoid SpawnFirstLayerObjects()
        {
            await UniTask.Delay(2000);

            foreach (var envObj in _config.firstParallaxLayer.environmentObjects)
            {
                for (var i = 0; i < 3; i++)
                {
                    var firstLayerObject = _spawner.SpawnById(envObj.id, envObj.layerType,
                        new Vector3(Random.Range(0, 5), Random.Range(0, 5), 0));
                    if (_firstLayerEnvironmentObjects.Contains(firstLayerObject)) continue;
                    _firstLayerEnvironmentObjects.Add(firstLayerObject);
                    firstLayerObject.gameObject.SetActive(true);
                }
            }
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}