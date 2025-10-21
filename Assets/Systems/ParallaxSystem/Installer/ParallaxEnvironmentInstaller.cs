using Systems.ParallaxSystem.Controller;
using Systems.ParallaxSystem.Handler;
using Systems.ParallaxSystem.Model;
using Systems.ParallaxSystem.ObjectPooling;
using Systems.ParallaxSystem.View;
using UnityEngine;
using Zenject;

namespace Systems.ParallaxSystem.Installer
{
    [CreateAssetMenu(fileName = "ParallaxEnvironmentInstaller", menuName = "Installers/ParallaxEnvironmentInstaller")]
    public class ParallaxEnvironmentInstaller : ScriptableObjectInstaller<ParallaxEnvironmentInstaller>
    {
        [SerializeField] private ParallaxEnvironmentView parallaxEnvironmentView;
        
        [SerializeField] private FirstLayerEnvironmentObject firstLayerPrefab;
        [SerializeField] private SecondLayerEnvironmentObject secondLayerPrefab;
        [SerializeField] private ThirdLayerEnvironmentObject thirdLayerPrefab;
        [SerializeField] private FourthLayerEnvironmentObject fourthLayerPrefab;
        // [SerializeField] private FifthLayerEnvironmentObject fifthLayerPrefab;

        public override void InstallBindings()
        {
            // View
            Container.Bind<ParallaxEnvironmentView>().FromComponentInNewPrefab(parallaxEnvironmentView).AsSingle();
            
            Container.BindMemoryPool<FirstLayerEnvironmentObject, FirstLayerPool>()
                .WithInitialSize(10)
                .FromComponentInNewPrefab(firstLayerPrefab)
                .UnderTransformGroup("Environment/FirstLayer");

            Container.BindMemoryPool<SecondLayerEnvironmentObject, SecondLayerPool>()
                .WithInitialSize(8)
                .FromComponentInNewPrefab(secondLayerPrefab)
                .UnderTransformGroup("Environment/SecondLayer");

            Container.BindMemoryPool<ThirdLayerEnvironmentObject, ThirdLayerPool>()
                .WithInitialSize(6)
                .FromComponentInNewPrefab(thirdLayerPrefab)
                .UnderTransformGroup("Environment/ThirdLayer");

            Container.BindMemoryPool<FourthLayerEnvironmentObject, FourthLayerPool>()
                .WithInitialSize(4)
                .FromComponentInNewPrefab(fourthLayerPrefab)
                .UnderTransformGroup("Environment/FourthLayer");

            // Container.BindMemoryPool<FifthLayerEnvironmentObject, FifthLayerPool>()
            //     .WithInitialSize(2)
            //     .FromComponentInNewPrefab(fifthLayerPrefab)
            //     .UnderTransformGroup("Environment/FifthLayer");

            Container.BindInterfacesTo<ParallaxEnvironmentSpawner>().AsSingle();
            
            // Controller
            Container.Bind<ParallaxEnvironmentController>().AsSingle();
        }
    }
}