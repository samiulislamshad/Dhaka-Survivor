using Systems.EnemySystem.EnemyFactory;
using Systems.EnemySystem.Model;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;
using Zenject.SpaceFighter;

namespace Systems.EnemySystem.Installer
{
    [CreateAssetMenu(fileName = "EnemyInstaller", menuName = "Installer/EnemyInstaller")]
    public class EnemyInstaller : ScriptableObjectInstaller<EnemyInstaller>
    {
        [FormerlySerializedAs("_meleeEnemyPrefab")]
        [Header("Enemy Prefabs")]
        [SerializeField] private MeleeEnemy meleeEnemyPrefab;
        [SerializeField] private RangedEnemy rangedEnemyPrefab;
        [SerializeField] private FlyingEnemy flyingEnemyPrefab;
        
        [Header("Spawner Settings")]
        [SerializeField] private EnemySpawner.Settings spawnerSettings;
        
        public override void InstallBindings()
        {
            BindEnemyPools();
            BindSpawner();
        }
        
        private void BindEnemyPools()
        {
            // Bind each enemy pool with different sizes based on enemy type
            Container.BindMemoryPool<MeleeEnemy, MeleeEnemyPool>()
                .WithInitialSize(10)
                .WithMaxSize(30)
                .FromComponentInNewPrefab(meleeEnemyPrefab)
                .UnderTransformGroup("Enemies");

            Container.BindMemoryPool<RangedEnemy, RangedEnemyPool>()
                .WithInitialSize(5)
                .WithMaxSize(15)
                .FromComponentInNewPrefab(rangedEnemyPrefab)
                .UnderTransformGroup("Enemies");

            Container.BindMemoryPool<FlyingEnemy, FlyingEnemyPool>()
                .WithInitialSize(3)
                .WithMaxSize(10)
                .FromComponentInNewPrefab(flyingEnemyPrefab)
                .UnderTransformGroup("Enemies");
        }
        
        private void BindSpawner()
        {
            Container.BindInstance(spawnerSettings).WhenInjectedInto<EnemySpawner>();
            Container.BindInterfacesTo<EnemySpawner>().AsSingle();
        }
    }
}