using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Systems.ParallaxSystem.Model
{
    public class SecondLayerEnvironmentObject : EnvironmentObject
    {
        // [SerializeField] private float parallaxSpeed = 0.3f;
    
        public override void Initialize(EnvironmentObjectData data, Vector3 position, CancellationToken cancellationToken = default)
        {
            transform.position = position;
            Id = data.id;
            Data = data;
        }

        public override void Reinitialize(Vector3 pos)
        {
            transform.position = pos;
        }

        public override void OnFixedUpdate(float gameSpeed)
        {
            
        }

        public override bool ShouldDespawn()
        {
            return transform.position.x < -25f;
        }
    }
}