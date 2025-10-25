using System.Threading;
using Systems.EnemySystem.Model;
using UnityEngine;

namespace Systems.ParallaxSystem.Model
{
    public class FirstLayerEnvironmentObject : EnvironmentObject
    {
        [SerializeField] private WindowAunty windowAunty;
        
        public override void Initialize(EnvironmentObjectData data, Vector3 position, CancellationToken cancellationToken = default)
        {
            transform.position = position;
            Id = data.id;
            Data = data;
            parallaxSpeed = 1f;

            InitializeWindowAunty();
        }

        public override void Reinitialize(Vector3 pos)
        {
            transform.position = pos;
            InitializeWindowAunty();
        }

        public override void OnFixedUpdate(float gameSpeed)
        {
            var movement = new Vector2(-gameSpeed, 0) * (Time.fixedDeltaTime * parallaxSpeed);
            transform.Translate(movement);
            
            if(windowAunty != null)
                if(transform.position.x <= 20)
                    windowAunty.OnUpdate();
        }

        public override bool ShouldDespawn()
        {
            // Implement despawn logic based on camera position or other criteria
            return transform.position.x < -100f; // Example condition
        }

        private void InitializeWindowAunty()
        {
            if (windowAunty == null) return;
            if(!windowAunty.isDead) return;
            windowAunty.Initialize();
        }
    }
}