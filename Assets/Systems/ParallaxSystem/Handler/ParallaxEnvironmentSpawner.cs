using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Systems.ParallaxSystem.Config;
using Systems.ParallaxSystem.Enum;
using Systems.ParallaxSystem.Model;
using Systems.ParallaxSystem.ObjectPooling;
using UniRx;
using UnityEngine;
using Zenject;

namespace Systems.ParallaxSystem.Handler
{
    [Serializable]
    public class ParallaxEnvironmentSpawner : ITickable, IInitializable, IDisposable
    {
        private readonly FirstLayerPool _firstLayerPool;
        private readonly SecondLayerPool _secondLayerPool;
        // private readonly ThirdLayerPool _thirdLayerPool;
        // private readonly FourthLayerPool _fourthLayerPool;
        private readonly ParallaxLayerConfig _config;

        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        private CancellationTokenSource _cancellationTokenSource;

        public ParallaxEnvironmentSpawner(
            FirstLayerPool firstLayerPool,
            SecondLayerPool secondLayerPool,
            // ThirdLayerPool thirdLayerPool,
            // FourthLayerPool fourthLayerPool,
            ParallaxLayerConfig config)
        {
            _firstLayerPool = firstLayerPool;
            _secondLayerPool = secondLayerPool;
            // _thirdLayerPool = thirdLayerPool;
            // _fourthLayerPool = fourthLayerPool;
            _config = config;
        }

        public void Initialize()
        {
            _cancellationTokenSource = new CancellationTokenSource();

            // Initialize pools with pre-spawned objects
            _firstLayerPool.InitializePool(3);
            _secondLayerPool.InitializePool(3);
            // _thirdLayerPool.InitializePool(3);
            // _fourthLayerPool.InitializePool(3);

            // Start background tasks for each layer if needed
            StartLayerSpawning().Forget();
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _disposables?.Dispose();

            // Dispose pools
            _firstLayerPool?.Dispose();
            _secondLayerPool?.Dispose();
            // _thirdLayerPool?.Dispose();
            // _fourthLayerPool?.Dispose();
        }

        // Spawn random object from layer
        public EnvironmentObject Spawn(EnvironmentLayerType layerType, Vector3 position)
        {
            return layerType switch
            {
                EnvironmentLayerType.First => _firstLayerPool.Spawn(position),
                EnvironmentLayerType.Second => _secondLayerPool.Spawn(position),
                // EnvironmentLayerType.Third => _thirdLayerPool.Spawn(position),
                // EnvironmentLayerType.Fourth => _fourthLayerPool.Spawn(position),
                _ => null
            };
        }

        // Spawn specific object by ID
        public EnvironmentObject SpawnById(string id, EnvironmentLayerType layerType, Vector3 position)
        {
            return layerType switch
            {
                EnvironmentLayerType.First => _firstLayerPool.SpawnById(id, position),
                EnvironmentLayerType.Second => _secondLayerPool.SpawnById(id, position),
                // EnvironmentLayerType.Third => _thirdLayerPool.SpawnById(id, position),
                // EnvironmentLayerType.Fourth => _fourthLayerPool.SpawnById(id, position),
                _ => null
            };
        }

        public void Despawn(EnvironmentLayerType layerType, EnvironmentObject environmentObject)
        {
            switch (layerType)
            {
                case EnvironmentLayerType.First:
                    if (environmentObject is FirstLayerEnvironmentObject firstLayer)
                        _firstLayerPool.Despawn(firstLayer);
                    break;
                case EnvironmentLayerType.Second:
                    if (environmentObject is SecondLayerEnvironmentObject secondLayer)
                        _secondLayerPool.Despawn(secondLayer);
                    break;
                // case EnvironmentLayerType.Third:
                //     if (environmentObject is ThirdLayerEnvironmentObject thirdLayer)
                //         _thirdLayerPool.Despawn(thirdLayer);
                //     break;
                // case EnvironmentLayerType.Fourth:
                //     if (environmentObject is FourthLayerEnvironmentObject fourthLayer)
                //         _fourthLayerPool.Despawn(fourthLayer);
                //     break;
            }
        }

        public void Tick()
        {
            CleanPool<FirstLayerEnvironmentObject>(EnvironmentLayerType.First);
            CleanPool<SecondLayerEnvironmentObject>(EnvironmentLayerType.Second);
            // CleanPool<ThirdLayerEnvironmentObject>(EnvironmentLayerType.Third);
            // CleanPool<FourthLayerEnvironmentObject>(EnvironmentLayerType.Fourth);
        }

        private void CleanPool<T>(EnvironmentLayerType layerType) where T : EnvironmentObject
        {
            var environmentObjects = UnityEngine.Object.FindObjectsOfType<T>();
            foreach (var environmentObject in environmentObjects)
            {
                if (environmentObject.gameObject.activeSelf && environmentObject.ShouldDespawn())
                {
                    Despawn(layerType, environmentObject);
                }
            }
        }

        private async UniTaskVoid StartLayerSpawning()
        {
            var token = _cancellationTokenSource.Token;

            try
            {
                // Example: Spawn initial environment objects for each layer
                await UniTask.WhenAll(
                    SpawnInitialLayerObjects(EnvironmentLayerType.First, 5, token),
                    SpawnInitialLayerObjects(EnvironmentLayerType.Second, 3, token)
                    // SpawnInitialLayerObjects(EnvironmentLayerType.Third, 4, token),
                    // SpawnInitialLayerObjects(EnvironmentLayerType.Fourth, 2, token)
                );
            }
            catch (OperationCanceledException)
            {
                // Expected when token is cancelled
            }
        }

        private async UniTask SpawnInitialLayerObjects(EnvironmentLayerType layerType, int count,
            CancellationToken token)
        {
            for (var i = 0; i < count; i++)
            {
                if (token.IsCancellationRequested)
                    return;

                var position = CalculateSpawnPosition(layerType, i);
                Spawn(layerType, position);

                await UniTask.Delay(100, cancellationToken: token); // Stagger spawns
            }
        }

        private Vector3 CalculateSpawnPosition(EnvironmentLayerType layerType, int index)
        {
            float xPos = index * 10f; // Adjust based on your needs
            float yPos = 0f;
            float zPos = (int)layerType * -1f; // Different Z positions for parallax layers

            return new Vector3(xPos, yPos, zPos);
        }
    }
}