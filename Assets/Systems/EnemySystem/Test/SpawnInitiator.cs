using Systems.EnemySystem.Enum;
using Systems.EnemySystem.Service;
using Systems.PlayerSystem.Signals;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace Systems.EnemySystem.Test
{
    public class SpawnInitiator : MonoBehaviour
    {
        private EnemySpawner _spawner;
        private SignalBus _signalBus;

        [SerializeField] private bool isRandom;
        [SerializeField] private EnemyType enemyType;

        [Inject]
        public void Construct(EnemySpawner spawner, SignalBus signalBus)
        {
            _spawner = spawner;
            _signalBus = signalBus;
        }

        private void Start()
        {
            SubscribeToActions();
        }

        private void SubscribeToActions()
        {
            _signalBus.Subscribe<SpawnEnemySignal>(SpawnEnemies);
        }

        private void SpawnEnemies(SpawnEnemySignal signal)
        {
            _spawner.Spawn(isRandom ? (EnemyType)Random.Range(0, 3) : enemyType, transform.position);
        }

        private void OnDestroy()
        {
            _signalBus.Unsubscribe<SpawnEnemySignal>(SpawnEnemies);
        }
    }
}