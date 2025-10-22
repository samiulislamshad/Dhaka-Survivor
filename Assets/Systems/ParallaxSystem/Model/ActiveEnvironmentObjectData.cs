using System;
using Systems.ParallaxSystem.Enum;

namespace Systems.ParallaxSystem.Model
{
    [Serializable]
    public class ActiveEnvironmentObjectData
    {
        public string guid; // Unique instance identifier
        public string id; // Original object data ID
        public EnvironmentLayerType layerType;
        public float timeElapsedSinceSpawn;
        public float estimatedSpawnTime;

        public ActiveEnvironmentObjectData(EnvironmentObjectData data)
        {
            guid = Guid.NewGuid().ToString();
            layerType = data.layerType;
            id = data.id;
            estimatedSpawnTime = data.estimatedSpawnTime;
            timeElapsedSinceSpawn = 0f;
        }
    }
}