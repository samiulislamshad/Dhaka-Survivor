using System.Threading;
using Systems.ParallaxSystem.Interface;
using UnityEngine;

namespace Systems.ParallaxSystem.Model
{
    public abstract class EnvironmentObject : MonoBehaviour, IEnvironmentObject
    {
        public string Id { get; protected set; }
        public float parallaxSpeed;
        protected EnvironmentObjectData Data;
        public abstract void Initialize(EnvironmentObjectData data, Vector3 position, CancellationToken cancellationToken = default);
        public abstract void Reinitialize(Vector3 pos);
        public abstract void OnFixedUpdate(float gameSpeed);
        public abstract bool ShouldDespawn();

        public virtual void OnSpawned()
        {
            // gameObject.SetActive(true);
        }

        public virtual void OnDespawned()
        {
            gameObject.SetActive(false);
        }
    }
}