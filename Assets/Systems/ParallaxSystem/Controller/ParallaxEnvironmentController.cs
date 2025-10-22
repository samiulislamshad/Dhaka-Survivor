using System;
using Cysharp.Threading.Tasks;
using Systems.GameSystem.Config;
using Systems.ParallaxSystem.Config;
using Systems.ParallaxSystem.Handler;
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
        
        private GameConfig  _gameConfig;
        private ParallaxLayerConfig _config;

        public ParallaxEnvironmentController(ParallaxEnvironmentSpawner spawner, GameConfig gameConfig, ParallaxLayerConfig config)
        {
            _spawner = spawner;
            _gameConfig = gameConfig;
            _config = config;

            _disposable = new CompositeDisposable();
            
            SpawnFirstLayerObjects().Forget();
        }

        public void FixedTick()
        {
            
        }

        private async UniTaskVoid SpawnFirstLayerObjects()
        {
            await UniTask.Delay(2000);

            foreach (var envObj in _config.firstParallaxLayer.environmentObjects)
            {
                for (var i = 0; i < 3; i++)
                {
                    _spawner.SpawnById(envObj.id, envObj.layerType, new Vector3(Random.Range(0, 5), Random.Range(0, 5), 0));
                }
            }
        }
        
        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}