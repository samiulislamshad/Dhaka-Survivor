using Systems.PlayerSystem.Controller;
using Systems.PlayerSystem.Signals;
using Systems.PlayerSystem.Signals.GameSignals;
using UnityEngine;
using Zenject;

namespace Systems.PlayerSystem.Installer
{
    [CreateAssetMenu(fileName = "PlayerInstaller", menuName = "Installers/PlayerInstaller")]
    public class PlayerInstaller : ScriptableObjectInstaller<PlayerInstaller>
    {
        [SerializeField] private PlayerController playerController;
        public override void InstallBindings()
        {
            // Signals
            Container.DeclareSignal<AttackInputSignal>();
            Container.DeclareSignal<StartJumpInputSignal>();
            Container.DeclareSignal<StopJumpInputSignal>();
            Container.DeclareSignal<StartCrouchInputSignal>();
            Container.DeclareSignal<StopCrouchInputSignal>();
            Container.DeclareSignal<TogglePauseInputSignal>();
            Container.DeclareSignal<SpawnEnemySignal>();

            Container.DeclareSignal<ContactWithEnemySignal>();
            Container.DeclareSignal<PlayerDeadSignal>();
            Container.DeclareSignal<PlayerSpecialJumpSignal>();

            Container.BindInterfacesAndSelfTo<PlayerController>().FromComponentInNewPrefab(playerController).AsSingle().NonLazy();
        }
    }
}