using Services;
using Systems.GameSystem.Config;
using Systems.GameSystem.Manager;
using Systems.GameSystem.Signals;
using Systems.GameSystem.View;
using UnityEngine;
using Zenject;

namespace Systems.GameSystem.Installer
{
    [CreateAssetMenu(fileName = "GameInstaller",  menuName = "Installers/GameInstaller")]
    public class GameInstaller : ScriptableObjectInstaller<GameInstaller>
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private GameConfig gameConfig;
        [SerializeField] private StartGameCanvasView startGameCanvasView;
        [SerializeField] private Camera mainCameraPrefab;
        
        public override void InstallBindings()
        {
            SignalBusInstaller.Install(Container);

            Container.DeclareSignal<MainMenuScreenSignal>();
            Container.DeclareSignal<NameInputSignal>();
            Container.DeclareSignal<GameScreenSignal>();
            Container.DeclareSignal<ScoreBoardSignal>();
            
            Container.Bind<InputMaster>()
                .AsSingle()
                .NonLazy();
            
            // Prefab
            Container.Bind<Camera>()
                .WithId("MainCamera")
                .FromComponentInNewPrefab(mainCameraPrefab)
                .AsSingle()
                .NonLazy();
            
            // Services
            Container.BindInterfacesAndSelfTo<SceneLoaderService>().AsSingle();
            
            // Config
            Container.Bind<GameConfig>().FromScriptableObject(gameConfig).AsSingle();
            // View
            Container.Bind<StartGameCanvasView>().FromComponentInNewPrefab(startGameCanvasView).AsSingle().NonLazy();

            // Manager
            Container.Bind<GameManager>().FromComponentInNewPrefab(gameManager).AsSingle().NonLazy();
        }
    }
}