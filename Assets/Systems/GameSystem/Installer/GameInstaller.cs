using Services;
using Systems.AudioSystem.Manager;
using Systems.GameSystem.Config;
using Systems.GameSystem.Manager;
using Systems.GameSystem.Service;
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
        [SerializeField] private AudioManager audioManager;
        [SerializeField] private Camera mainCameraPrefab;
        
        // [SerializeField] private InactivityWarningUI inactivityWarningUI;
        
        public override void InstallBindings()
        {
            SignalBusInstaller.Install(Container);

            Container.DeclareSignal<MainMenuScreenSignal>();
            Container.DeclareSignal<NameInputSignal>();
            Container.DeclareSignal<GameScreenSignal>();
            Container.DeclareSignal<ScoreBoardSignal>();

            // Inactivity Detector
            // Container.Bind<InactivityWarningUI>().FromComponentInNewPrefab(inactivityWarningUI).AsSingle();
            // Container.BindInterfacesAndSelfTo<GameInactivityDetector>().AsSingle();
            
            Container.Bind<InputMaster>()
                .AsSingle()
                .NonLazy();
            
            // Prefab
            Container.Bind<Camera>()
                .WithId("MainCamera")
                .FromComponentInNewPrefab(mainCameraPrefab)
                .AsSingle()
                .NonLazy();

            Container.Bind<AudioManager>().FromComponentInNewPrefab(audioManager).AsSingle().NonLazy();
            
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