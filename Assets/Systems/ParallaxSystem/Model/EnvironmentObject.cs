using System;
using System.Threading;
using Systems.ParallaxSystem.Interface;
using UniRx;
using UnityEngine;

namespace Systems.ParallaxSystem.Model
{
    public abstract class EnvironmentObject : MonoBehaviour, IEnvironmentObject
    {
        public string Id { get; protected set; }
        public string Guid { get; set; } // Unique identifier for this instance
        public float parallaxSpeed;
        protected EnvironmentObjectData Data;
        // Signal to notify when this object despawns
        private Subject<string> _onDespawn = new();
        public IObservable<string> OnDespawnSignal => _onDespawn;
        
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
            _onDespawn.OnNext(Guid);
        }

        protected virtual void OnDestroy()
        {
            _onDespawn?.Dispose();
        }
    }
}