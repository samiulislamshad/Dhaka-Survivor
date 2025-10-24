using Systems.InputSystem.Controller;
using Systems.InputSystem.Signal;
using Systems.InputSystem.View;
using Systems.PlayerSystem.Signals.GameSignals;
using UnityEngine;
using Zenject;

namespace Systems.InputSystem.Installer
{
    [CreateAssetMenu(fileName = "VirtualKeyboardInstaller", menuName = "Installers/VirtualKeyboardInstaller")]
    public class VirtualKeyboardInstaller : ScriptableObjectInstaller<VirtualKeyboardInstaller>
    {
        [SerializeField] private VirtualKeyboardView view;

        public override void InstallBindings()
        {
            Container.DeclareSignal<SwitchToPlayerControlSignal>();
            Container.DeclareSignal<SwitchToUiControlSignal>();
            
            Container.DeclareSignal<SwitchOffPlayerControlSignal>();
            Container.DeclareSignal<SwitchOnPlayerControlSignal>();
            
            Container.Bind<VirtualKeyboardView>().FromComponentInNewPrefab(view).AsSingle();

            Container.Bind<VirtualKeyboardController>().AsSingle().NonLazy();
        }
    }
}