using Services;
using Systems.LoadingScreenSystem.Controller;
using Systems.LoadingScreenSystem.Model;
using Systems.LoadingScreenSystem.View;
using UnityEngine;
using Zenject;

namespace Systems.LoadingScreenSystem.Installer
{
    [CreateAssetMenu(fileName = "LoadingScreenInstaller", menuName = "Installers/LoadingScreenInstaller")]
    public class LoadingScreenInstaller : ScriptableObjectInstaller<LoadingScreenInstaller>
    {
        [SerializeField] private LoadingScreenCanvasView loadingScreenCanvasView;
        
        public override void InstallBindings()
        {
            // Services
            Container.BindInterfacesAndSelfTo<SceneLoaderService>().AsSingle();
            
            // View
            Container.Bind<LoadingScreenCanvasView>().FromComponentInNewPrefab(loadingScreenCanvasView).AsSingle();
            
            // Model
            Container.BindInterfacesAndSelfTo<LoadingScreenModel>().AsSingle();
            
            // Controller
            Container.BindInterfacesAndSelfTo<LoadingScreenController>().AsSingle();
        }
    }
}