using Systems.PlayerSystem.Signals;
using UnityEngine;
using Zenject;

namespace Systems.GameSystem.Installer
{
    [CreateAssetMenu(fileName = "GameInstaller",  menuName = "Installers/GameInstaller")]
    public class GameInstaller : ScriptableObjectInstaller<GameInstaller>
    {
        public override void InstallBindings()
        {
            SignalBusInstaller.Install(Container);

            Container.DeclareSignal<AttackInputSignal>();
            Container.DeclareSignal<StartJumpInputSignal>();
            Container.DeclareSignal<StopJumpInputSignal>();
            Container.DeclareSignal<StartCrouchInputSignal>();
            Container.DeclareSignal<StopCrouchInputSignal>();
            Container.DeclareSignal<TogglePauseInputSignal>();
        }
    }
}