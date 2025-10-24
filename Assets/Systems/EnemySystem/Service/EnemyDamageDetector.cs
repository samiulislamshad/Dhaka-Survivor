using System;
using UniRx;
using UnityEngine;

namespace Systems.EnemySystem.Service
{
    public class EnemyDamageDetector : MonoBehaviour
    {
        private readonly Subject<Collision2D> _onCollisionEnter2d = new();
        public IObservable<Collision2D> OnCollisionEnter2d => _onCollisionEnter2d;
        

        private void OnCollisionEnter2D(Collision2D other)
        {
            if(!other.gameObject.CompareTag("Player")) return;
            _onCollisionEnter2d.OnNext(other);
            gameObject.SetActive(false);
        }
    }
}