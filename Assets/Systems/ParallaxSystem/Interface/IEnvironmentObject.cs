using System.Threading;
using Systems.ParallaxSystem.Model;
using UnityEngine;

namespace Systems.ParallaxSystem.Interface
{
    public interface IEnvironmentObject
    {
        void Initialize(EnvironmentObjectData data, Vector3 position, CancellationToken cancellationToken = default);
        void Reinitialize(Vector3 pos);
        bool ShouldDespawn();
        void OnSpawned();
        void OnDespawned();
    }
}