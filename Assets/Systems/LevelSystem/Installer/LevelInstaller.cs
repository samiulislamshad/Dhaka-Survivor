using Systems.LevelSystem.Controller;
using Systems.LevelSystem.View;
using UnityEngine;
using Zenject;

namespace Systems.LevelSystem.Installer
{
    [CreateAssetMenu(fileName = "LevelInstaller", menuName = "Installers/LevelInstaller")]
    public class LevelInstaller : ScriptableObjectInstaller<LevelInstaller>
    {
        [SerializeField] private LevelView levelView;

        public override void InstallBindings()
        {
            // View
            Container.Bind<LevelView>().FromComponentInNewPrefab(levelView).AsSingle();
            
            // Controller
            Container.Bind<LevelController>().AsSingle();
        }
    }
}