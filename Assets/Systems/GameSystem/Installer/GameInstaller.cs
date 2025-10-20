using Systems.GameSystem.Config;
using Systems.GameSystem.Manager;
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
            
            // Prefab
            Container.Bind<Camera>()
                .WithId("MainCamera")
                .FromComponentInNewPrefab(mainCameraPrefab)
                .AsSingle()
                .NonLazy();
            
            // Config
            Container.Bind<GameConfig>().FromScriptableObject(gameConfig).AsSingle();
            // View
            Container.Bind<StartGameCanvasView>().FromComponentInNewPrefab(startGameCanvasView).AsSingle().NonLazy();

            // Manager
            Container.Bind<GameManager>().FromComponentInNewPrefab(gameManager).AsSingle().NonLazy();
        }
    }
}