using Systems.MainMenuSystem.Controller;
using Systems.MainMenuSystem.Model;
using Systems.MainMenuSystem.View;
using UnityEngine;
using Zenject;

namespace Systems.MainMenuSystem.Installer
{
    [CreateAssetMenu(fileName = "MainMenuCanvasInstaller", menuName = "Installers/MainMenuCanvasInstaller")]
    public class MainMenuCanvasInstaller : ScriptableObjectInstaller<MainMenuCanvasInstaller>
    {
        [SerializeField] private MainMenuCanvasView view;

        public override void InstallBindings()
        {
            //View
            Container.Bind<MainMenuCanvasView>().FromComponentInNewPrefab(view).AsSingle();

            //Model
            Container.BindInterfacesAndSelfTo<MainMenuCanvasModel>().AsSingle();

            //Controller
            Container.BindInterfacesAndSelfTo<MainMenuCanvasController>().AsSingle();
        }
    }
}