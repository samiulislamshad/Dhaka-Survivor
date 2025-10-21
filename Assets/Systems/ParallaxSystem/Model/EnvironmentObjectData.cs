using System;
using System.Collections.Generic;
using Systems.ParallaxSystem.Enum;
using UnityEngine;

namespace Systems.ParallaxSystem.Model
{
    [Serializable]
    public class EnvironmentObjectData
    {
        public string id;
        public string name;
        public EnvironmentLayerType layerType;
        public List<RelativeObjectData> crucialRelativeObjects;
        public List<RelativeObjectData> preferredRelativeObjects;
        public float estimatedSpawnTime;
        public GameObject prefab;
    }
}