using Systems.PauseSystem.Controller;
using Systems.PauseSystem.Signals;
using UnityEngine;
using Zenject;

namespace Systems.PauseSystem.Installer
{
    [CreateAssetMenu(fileName = "PauseInstaller", menuName = "Installers/PauseInstaller")]
    public class PauseInstaller : ScriptableObjectInstaller<PauseInstaller>
    {
        public override void InstallBindings()
        {
            Container.DeclareSignal<PauseSignal>();
            Container.DeclareSignal<UnpauseSignal>();

            Container.Bind<PauseController>().AsSingle().NonLazy();
        }
    }
}