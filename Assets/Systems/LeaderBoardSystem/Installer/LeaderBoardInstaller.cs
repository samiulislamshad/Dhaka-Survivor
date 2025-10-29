using Systems.GameSystem.Config;
using Systems.LeaderBoardSystem.Controller;
using Systems.LeaderBoardSystem.Manager;
using Systems.LeaderBoardSystem.Model;
using Systems.LeaderBoardSystem.Scriptable;
using Systems.LeaderBoardSystem.Signal;
using Systems.LeaderBoardSystem.View;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace Systems.LeaderBoardSystem.Installer
{
    [CreateAssetMenu(fileName = "LeaderBoardInstaller", menuName = "Installers/LeaderBoardInstaller")]
    public class LeaderBoardInstaller : ScriptableObjectInstaller<LeaderBoardInstaller>
    {
        [SerializeField] private EventSystem eventSystem;
        [SerializeField] private LeaderBoardCanvasView canvasView;
        [SerializeField] private LeaderBoardScriptable leaderBoardScriptable;
        [SerializeField] private LeaderBoardController controller;
        [SerializeField] private GameConfig gameConfig;
        
        public override void InstallBindings()
        {
            SignalBusInstaller.Install(Container);
            
            // Signals
            Container.DeclareSignal<ScrollNavigationSignal>();
            
            Container.Bind<InputMaster>().AsSingle().NonLazy();
            Container.Bind<EventSystem>().FromComponentInNewPrefab(eventSystem).AsSingle();
            Container.Bind<GameConfig>().FromScriptableObject(gameConfig).AsSingle();
            Container.Bind<LeaderBoardScriptable>().FromScriptableObject(leaderBoardScriptable).AsSingle();
            
            // View
            Container.Bind<LeaderBoardCanvasView>().FromComponentInNewPrefab(canvasView).AsSingle();
            
            // Manager
            Container.Bind<LeaderboardManager>().AsSingle();
            
            // Model
            Container.Bind<LeaderBoardModel>().AsSingle();
            
            // Controller
            Container.Bind<LeaderBoardController>().FromComponentInNewPrefab(controller).AsSingle().NonLazy();
        }
    }
}