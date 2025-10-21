using System.Threading;
using Cysharp.Threading.Tasks;
using Systems.ParallaxSystem.Interface;
using UnityEngine;

namespace Systems.ParallaxSystem.Model
{
    public abstract class EnvironmentObject : MonoBehaviour, IEnvironmentObject
    {
        public abstract UniTask Initialize(Vector3 position, CancellationToken cancellationToken = default);
        public abstract bool ShouldDespawn();

        public virtual void OnSpawned()
        {
            gameObject.SetActive(true);
        }

        public virtual void OnDespawned()
        {
            gameObject.SetActive(false);
        }
    }
}