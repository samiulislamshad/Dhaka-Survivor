using UnityEngine;
using Zenject;

namespace Systems.PauseSystem.Installer
{
    [CreateAssetMenu(fileName = "PauseInstaller", menuName = "Installers/PauseInstaller")]
    public class PauseInstaller : ScriptableObjectInstaller<PauseInstaller>
    {
        public override void InstallBindings()
        {
            
        }
    }
}