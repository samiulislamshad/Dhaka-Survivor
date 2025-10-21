using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Systems.ParallaxSystem.Model
{
    public class SecondLayerEnvironmentObject : EnvironmentObject
    {
        [SerializeField] private float parallaxSpeed = 0.3f;
    
        public override async UniTask Initialize(Vector3 position, CancellationToken cancellationToken = default)
        {
            transform.position = position;
            // Add any second layer specific initialization
            await UniTask.CompletedTask;
        }
    
        public override bool ShouldDespawn()
        {
            return transform.position.x < -25f;
        }
    }
}