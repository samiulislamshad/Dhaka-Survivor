using System;
using Systems.ParallaxSystem.Model;
using UnityEngine;
using Zenject;

namespace Systems.ParallaxSystem.Factory
{
    [Serializable]
    public class ThirdLayerEnvironmentObjectFactory : PlaceholderFactory<EnvironmentObjectData, Vector3, ThirdLayerEnvironmentObject>
    {
        
    }
}