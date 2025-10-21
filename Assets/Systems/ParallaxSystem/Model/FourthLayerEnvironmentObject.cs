using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Systems.ParallaxSystem.Model
{
    [Serializable]
    public class FourthLayerEnvironmentObject : EnvironmentObject
    {
        [SerializeField] private float parallaxSpeed = 0.1f;
    
        public override async UniTask Initialize(Vector3 position, CancellationToken cancellationToken = default)
        {
            transform.position = position;
            // Add any first layer specific initialization
            await UniTask.CompletedTask;
        }
    
        public override bool ShouldDespawn()
        {
            // Implement despawn logic based on camera position or other criteria
            return transform.position.x < -20f; // Example condition
        }
    }
}