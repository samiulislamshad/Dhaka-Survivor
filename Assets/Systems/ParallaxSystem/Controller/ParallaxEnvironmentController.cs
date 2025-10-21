using System;
using Systems.GameSystem.Config;
using Systems.ParallaxSystem.Handler;
using Systems.ParallaxSystem.ObjectPooling;
using UniRx;
using Zenject;

namespace Systems.ParallaxSystem.Controller
{
    [Serializable]
    public class ParallaxEnvironmentController : IFixedTickable, IDisposable
    {
        private ParallaxEnvironmentSpawner _spawner;
        private CompositeDisposable _disposable;
        
        private GameConfig  _gameConfig;

        public void FixedTick()
        {
            
        }

        private void SpawnFirstLayerObjects()
        {
            
        }
        
        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}