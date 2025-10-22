using System.Threading;
using UnityEngine;

namespace Systems.ParallaxSystem.Model
{
    public class FirstLayerEnvironmentObject : EnvironmentObject
    {
        // [SerializeField] private float parallaxSpeed = 0.1f;
    
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
            var movement = new Vector2(-gameSpeed, 0) * Time.fixedDeltaTime;
            transform.Translate(movement);
        }

        public override bool ShouldDespawn()
        {
            // Implement despawn logic based on camera position or other criteria
            return transform.position.x < -60f; // Example condition
        }
        
        public float GetParallaxSpeed() => parallaxSpeed;
    }
}