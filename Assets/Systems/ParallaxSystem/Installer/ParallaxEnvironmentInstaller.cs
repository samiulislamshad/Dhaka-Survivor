using Systems.ParallaxSystem.Config;
using Systems.ParallaxSystem.Controller;
using Systems.ParallaxSystem.Factory;
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
        [SerializeField] private ParallaxLayerConfig parallaxLayerConfig;
        [SerializeField] private ParallaxEnvironmentView parallaxEnvironmentView;

        [SerializeField] private FirstLayerEnvironmentObject firstLayerPrefab;
        [SerializeField] private SecondLayerEnvironmentObject secondLayerPrefab;
        [SerializeField] private ThirdLayerEnvironmentObject thirdLayerPrefab;
        [SerializeField] private FourthLayerEnvironmentObject fourthLayerPrefab;
        // [SerializeField] private FifthLayerEnvironmentObject fifthLayerPrefab;

        public override void InstallBindings()
        {
            // Config
            Container.Bind<ParallaxLayerConfig>().FromScriptableObject(parallaxLayerConfig).AsSingle();

            // View
            // Container.Bind<ParallaxEnvironmentView>().FromComponentInNewPrefab(parallaxEnvironmentView).AsSingle();

            // Create parent transforms
            var envParent = new GameObject("Environment").transform;

            var firstLayerParent = new GameObject("FirstLayer").transform;
            firstLayerParent.SetParent(envParent);

            var secondLayerParent = new GameObject("SecondLayer").transform;
            secondLayerParent.SetParent(envParent);

            var thirdLayerParent = new GameObject("ThirdLayer").transform;
            thirdLayerParent.SetParent(envParent);

            var fourthLayerParent = new GameObject("FourthLayer").transform;
            fourthLayerParent.SetParent(envParent);

            // Bind parent transforms
            Container.Bind<Transform>()
                .WithId("FirstLayerParent")
                .FromInstance(firstLayerParent)
                .AsCached();

            Container.Bind<Transform>()
                .WithId("SecondLayerParent")
                .FromInstance(secondLayerParent)
                .AsCached();

            Container.Bind<Transform>()
                .WithId("ThirdLayerParent")
                .FromInstance(thirdLayerParent)
                .AsCached();

            Container.Bind<Transform>()
                .WithId("FourthLayerParent")
                .FromInstance(fourthLayerParent)
                .AsCached();

            // Bind factories
            Container
                .BindFactory<EnvironmentObjectData, Vector3, FirstLayerEnvironmentObject,
                    FirstLayerEnvironmentObjectFactory>()
                .FromFactory<FirstLayerEnvironmentObjectCustomFactory>();

            Container
                .BindFactory<EnvironmentObjectData, Vector3, SecondLayerEnvironmentObject,
                    SecondLayerEnvironmentObjectFactory>()
                .FromFactory<SecondLayerEnvironmentObjectCustomFactory>();

            // Container
            //     .BindFactory<EnvironmentObjectData, Vector3, ThirdLayerEnvironmentObject,
            //         ThirdLayerEnvironmentObjectFactory>()
            //     .FromFactory<ThirdLayerEnvironmentObjectCustomFactory>();
            //
            // Container
            //     .BindFactory<EnvironmentObjectData, Vector3, FourthLayerEnvironmentObject,
            //         FourthLayerEnvironmentObjectFactory>()
            //     .FromFactory<FourthLayerEnvironmentObjectCustomFactory>();

            // Bind pools
            Container.Bind<FirstLayerPool>()
                .AsSingle();

            Container.Bind<SecondLayerPool>()
                .AsSingle();

            // Container.Bind<ThirdLayerPool>()
            //     .AsSingle();
            //
            // Container.Bind<FourthLayerPool>()
            //     .AsSingle();

            // Bind spawner
            Container.BindInterfacesAndSelfTo<ParallaxEnvironmentSpawner>()
                .AsSingle()
                .NonLazy();

            // Controller
            Container.Bind<ParallaxEnvironmentController>().AsSingle();
        }
    }
}