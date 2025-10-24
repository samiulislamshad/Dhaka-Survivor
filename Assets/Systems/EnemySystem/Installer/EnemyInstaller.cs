using Systems.EnemySystem.Controller;
using Systems.EnemySystem.Model;
using Systems.EnemySystem.ObjectPool;
using Systems.EnemySystem.Service;
using Systems.EnemySystem.Signals;
using UnityEngine;
using Zenject;

namespace Systems.EnemySystem.Installer
{
    [CreateAssetMenu(fileName = "EnemyInstaller", menuName = "Installers/EnemyInstaller")]
    public class EnemyInstaller : ScriptableObjectInstaller<EnemyInstaller>
    {
        [SerializeField] private Chesra1Enemy chesra1EnemyPrefab;
        [SerializeField] private ChapriBikerEnemy chapriBikerEnemyPrefab;
        [SerializeField] private Chesra2Enemy chesra2EnemyPrefab;
        [SerializeField] private OfficeBossEnemy officeBossEnemyPrefab;
        [SerializeField] private AuntyEnemy auntyEnemyPrefab;
        [SerializeField] private Chesra3Enemy chesra3EnemyPrefab;
        [SerializeField] private PagriBroEnemy pagriBroEnemyPrefab;
        [SerializeField] private MinarEnemy minarEnemyPrefab;

        public override void InstallBindings()
        {
            // Signals
            Container.DeclareSignal<UnregisterEnemySignal>();
            
            // Bind melee enemy pool
            Container.BindMemoryPool<Chesra1Enemy, Chesra1Pool>()
                .WithInitialSize(5)
                .FromComponentInNewPrefab(chesra1EnemyPrefab)
                .UnderTransformGroup("Chesra1EnemyPool");

            // Bind ranged enemy pool
            Container.BindMemoryPool<ChapriBikerEnemy, ChapriBikerPool>()
                .WithInitialSize(5)
                .FromComponentInNewPrefab(chapriBikerEnemyPrefab)
                .UnderTransformGroup("ChapriBikerEnemyPool");

            // Bind aerial enemy pool
            Container.BindMemoryPool<Chesra2Enemy, Chesra2Pool>()
                .WithInitialSize(5)
                .FromComponentInNewPrefab(chesra2EnemyPrefab)
                .UnderTransformGroup("Chesra2EnemyPool");
            
            Container.BindMemoryPool<OfficeBossEnemy, OfficeBossPool>()
                .WithInitialSize(5)
                .FromComponentInNewPrefab(officeBossEnemyPrefab)
                .UnderTransformGroup("OfficeBossEnemyPool");
            
            Container.BindMemoryPool<AuntyEnemy, AuntyPool>()
                .WithInitialSize(5)
                .FromComponentInNewPrefab(auntyEnemyPrefab)
                .UnderTransformGroup("AuntyEnemyPool");
            
            Container.BindMemoryPool<PagriBroEnemy, PagriBroPool>()
                .WithInitialSize(5)
                .FromComponentInNewPrefab(pagriBroEnemyPrefab)
                .UnderTransformGroup("HojorEnemyPool");
            
            Container.BindMemoryPool<Chesra3Enemy, Chesra3Pool>()
                .WithInitialSize(5)
                .FromComponentInNewPrefab(chesra3EnemyPrefab)
                .UnderTransformGroup("Chesra3EnemyPool");
            
            Container.BindMemoryPool<MinarEnemy, MinarPool>()
                .WithInitialSize(5)
                .FromComponentInNewPrefab(minarEnemyPrefab)
                .UnderTransformGroup("MinarEnemyPool");

            // Bind spawner service
            Container.BindInterfacesAndSelfTo<EnemySpawner>().AsSingle();
            
            // Controller
            Container.BindInterfacesAndSelfTo<EnemyController>().AsSingle().NonLazy();
        }
    }
}