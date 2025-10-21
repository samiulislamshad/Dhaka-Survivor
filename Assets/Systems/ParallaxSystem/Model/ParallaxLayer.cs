using System;
using System.Collections.Generic;
using Systems.ParallaxSystem.Enum;

namespace Systems.ParallaxSystem.Model
{
    [Serializable]
    public class ParallaxLayer
    {
        public string id;
        public string layerName;
        public EnvironmentLayerType layerType;
        public List<EnvironmentObjectData> environmentObjects;
    }

        // public float baseSpeed;
        // public float parallaxMultiplier;
        // public float spawnInterval;
        // public List<float> spawnYPositions;
        // public float minSpacing;
        // public int maxActiveObjects;
        // public bool seamlessSpawn;
        // public float timeSinceLastSpawn;
}