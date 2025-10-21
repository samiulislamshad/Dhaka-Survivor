using System.Collections.Generic;
using Systems.ParallaxSystem.Model;
using UnityEngine;

namespace Systems.ParallaxSystem.Config
{
    [CreateAssetMenu(fileName = "ParallaxLayerConfig", menuName = "Configs/ParallaxLayerConfig")]
    public class ParallaxLayerConfig : ScriptableObject
    {
        public ParallaxLayer  firstParallaxLayer;
        public ParallaxLayer  secondParallaxLayer;
        public ParallaxLayer  thirdParallaxLayer;
        public ParallaxLayer  fourthParallaxLayer;
    }
}