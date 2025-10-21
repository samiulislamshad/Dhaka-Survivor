using System.Collections.Generic;
using Systems.ParallaxSystem.Model;
using UnityEngine;

namespace Systems.ParallaxSystem.Config
{
    [CreateAssetMenu(fileName = "ParallaxLayerConfig", menuName = "Configs/ParallaxLayerConfig")]
    public class ParallaxLayerConfig : ScriptableObject
    {
        public List<ParallaxLayer>  firstParallaxLayer;
        public List<ParallaxLayer>  secondParallaxLayer;
        public List<ParallaxLayer>  thirdParallaxLayer;
        public List<ParallaxLayer>  fourthParallaxLayer;
    }
}