using System.Threading;
using UnityEngine;

namespace Systems.ParallaxSystem.Model
{
    public class SecondLayerEnvironmentObject : EnvironmentObject
    {
        public override void Initialize(EnvironmentObjectData data, Vector3 position, CancellationToken cancellationToken = default)
        {
            transform.position = position;
            Id = data.id;
            Data = data;
            parallaxSpeed = 0.5f;
        }

        public override void Reinitialize(Vector3 pos)
        {
            transform.position = pos;
        }

        public override void OnFixedUpdate(float gameSpeed)
        {
            var movement = new Vector2(-gameSpeed, 0) * (Time.fixedDeltaTime * parallaxSpeed);
            transform.Translate(movement);
        }

        public override bool ShouldDespawn()
        {
            return transform.position.x < -60f;
        }
    }
}