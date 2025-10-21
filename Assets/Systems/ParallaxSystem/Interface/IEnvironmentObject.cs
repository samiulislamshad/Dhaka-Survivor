using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Systems.ParallaxSystem.Interface
{
    public interface IEnvironmentObject
    {
        UniTask Initialize(Vector3 position, CancellationToken cancellationToken = default);
        bool ShouldDespawn();
        void OnSpawned();
        void OnDespawned();
    }
}