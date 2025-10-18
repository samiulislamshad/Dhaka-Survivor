using Systems.EnemySystem.Config;
using Systems.EnemySystem.Model;
using Systems.EnemySystem.ObjectPool;
using Systems.EnemySystem.Service;
using UnityEngine;
using Zenject;

namespace Systems.EnemySystem.Installer
{
    [CreateAssetMenu(fileName = "EnemyInstaller", menuName = "Installers/EnemyInstaller")]
    public class EnemyInstaller : ScriptableObjectInstaller<EnemyInstaller>
    {
        [SerializeField] private EnemyConfig config;
        
        [SerializeField] private MeleeEnemy meleeEnemyPrefab;
        [SerializeField] private RangedEnemy rangedEnemyPrefab;
        [SerializeField] private AerialEnemy aerialEnemyPrefab;

        public override void InstallBindings()
        {
            // Bind melee enemy pool
            Container.BindMemoryPool<MeleeEnemy, MeleeEnemyPool>()
                .WithInitialSize(5)
                .FromComponentInNewPrefab(meleeEnemyPrefab)
                .UnderTransformGroup("MeleeEnemyPool");

            // Bind ranged enemy pool
            Container.BindMemoryPool<RangedEnemy, RangedEnemyPool>()
                .WithInitialSize(5)
                .FromComponentInNewPrefab(rangedEnemyPrefab)
                .UnderTransformGroup("RangedEnemyPool");

            // Bind aerial enemy pool
            Container.BindMemoryPool<AerialEnemy, AerialEnemyPool>()
                .WithInitialSize(5)
                .FromComponentInNewPrefab(aerialEnemyPrefab)
                .UnderTransformGroup("AerialEnemyPool");

            // Bind spawner service
            Container.BindInterfacesAndSelfTo<EnemySpawner>().AsSingle();
            
            // Config
            Container.Bind<EnemyConfig>().FromScriptableObject(config).AsSingle();
        }
    }
}