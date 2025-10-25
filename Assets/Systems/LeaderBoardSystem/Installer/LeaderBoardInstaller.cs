using Systems.LeaderBoardSystem.Controller;
using Systems.LeaderBoardSystem.Model;
using Systems.LeaderBoardSystem.View;
using UnityEngine;
using Zenject;

namespace Systems.LeaderBoardSystem.Installer
{
    [CreateAssetMenu(fileName = "LeaderBoardInstaller", menuName = "Installers/LeaderBoardInstaller")]
    public class LeaderBoardInstaller : ScriptableObjectInstaller<LeaderBoardInstaller>
    {
        [SerializeField] private LeaderBoardCanvasView canvasView;
        
        public override void InstallBindings()
        {
            // View
            Container.Bind<LeaderBoardCanvasView>().FromComponentInNewPrefab(canvasView).AsSingle();
            
            // Model
            Container.Bind<LeaderBoardModel>().AsSingle();
            
            // Controller
            Container.Bind<LeaderBoardController>().AsSingle().NonLazy();
        }
    }
}