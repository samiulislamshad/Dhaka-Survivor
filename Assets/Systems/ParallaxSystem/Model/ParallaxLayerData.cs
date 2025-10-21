using System;
using System.Collections.Generic;
using UnityEngine;

namespace Systems.ParallaxSystem.Model
{
    [Serializable]
    public class ParallaxLayerData
    {
        // [Header("Identification")]
        // public string layerName = "Background Layer";
        //
        // [Tooltip("Unique pool identifier")]
        // public string poolName = "bg_trees";
        //
        // [Header("Prefab & Pool")]
        // [Tooltip("Prefab to spawn (must have EnvironmentObject component)")]
        // public GameObject prefab;
        //
        // [Tooltip("Initial pool size (pre-instantiated objects)")]
        // [Range(1, 20)]
        // public int poolSize = 5;
        //
        // [Header("Movement Settings")]
        // [Tooltip("Base horizontal movement speed")]
        // public float baseSpeed = 0.5f;
        //
        // [Tooltip("Parallax depth multiplier (1.0 = same as enemies, <1.0 = background, >1.0 = foreground)")]
        // [Range(0.1f, 2f)]
        // public float parallaxMultiplier = 0.6f;
        //
        // [Header("Spawn Settings")]
        // [Tooltip("Time between spawns (if not seamless)")]
        // public float spawnInterval = 4f;
        //
        // [Tooltip("Possible Y positions for spawning")]
        // public List<float> spawnYPositions = new List<float> { 0f };
        //
        // [Tooltip("Minimum horizontal spacing between objects")]
        // public float minSpacing = 5f;
        //
        // [Tooltip("Maximum objects active at once")]
        // [Range(1, 10)]
        // public int maxActiveObjects = 4;
        //
        // [Tooltip("Enable seamless spawning (spawns when gap detected)")]
        // public bool seamlessSpawn = false;
        //
        // /// <summary>
        // /// Convert to runtime ParallaxLayer
        // /// </summary>
        // public ParallaxLayer ToParallaxLayer()
        // {
        //     return new ParallaxLayer
        //     {
        //         layerName = layerName,
        //         poolName = poolName,
        //         baseSpeed = baseSpeed,
        //         parallaxMultiplier = parallaxMultiplier,
        //         spawnInterval = spawnInterval,
        //         spawnYPositions = new List<float>(spawnYPositions),
        //         minSpacing = minSpacing,
        //         maxActiveObjects = maxActiveObjects,
        //         seamlessSpawn = seamlessSpawn,
        //         timeSinceLastSpawn = 0f
        //     };
        // }
    }
}