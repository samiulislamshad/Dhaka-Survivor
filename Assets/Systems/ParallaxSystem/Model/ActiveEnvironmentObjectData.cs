using System;
using Systems.ParallaxSystem.Enum;

namespace Systems.ParallaxSystem.Model
{
    [Serializable]
    public class ActiveEnvironmentObjectData
    {
        public string guid; // Unique identifier for this specific instance
        public string id; // Original object type ID
        public EnvironmentLayerType layerType;
        public float timeElapsedSinceSpawn;
        public float estimatedSpawnTime;

        public ActiveEnvironmentObjectData(EnvironmentObjectData data)
        {
            layerType = data.layerType;
            id = data.id;
            estimatedSpawnTime = data.estimatedSpawnTime;
            timeElapsedSinceSpawn = 0f;
            guid = System.Guid.NewGuid().ToString();
        }
    }
}