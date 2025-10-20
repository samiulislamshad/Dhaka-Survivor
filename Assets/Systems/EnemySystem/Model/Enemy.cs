using Systems.EnemySystem.Enum;
using Systems.EnemySystem.Interface;
using Systems.GameSystem.Config;
using UnityEngine;
using Zenject;

namespace Systems.EnemySystem.Model
{
    public abstract class Enemy : MonoBehaviour, IEnemy
    {
        [Inject] protected GameConfig Config;
        
        public abstract EnemyType Type { get; }

        [SerializeField] protected Rigidbody2D rb;
        [SerializeField] protected LayerMask despawnLayer;
        [SerializeField] protected LayerMask playerLayer;
        
        protected bool IsDespawning;
        protected float Timer;
        
        // ✨ ADD THESE NEW FIELDS ✨
        [Header("Movement Settings")]
        [SerializeField] protected float moveSpeedX = 0.5f;
        [SerializeField] protected float moveSpeedY = 0f;
    
        [Header("Parallax Settings")]
        [Tooltip("Should match the parallax layer (1.0 for foreground enemies)")]
        [SerializeField] protected float parallaxMultiplier = 1f;

        public virtual void Initialize(Vector3 position)
        {
            transform.position = position;
            Timer = 0f;
            IsDespawning = false;
            gameObject.SetActive(true);
        }

        private void Update()
        {
            Timer += Time.deltaTime;
            OnFixedUpdate();
        }

        protected abstract void OnFixedUpdate();

        public virtual bool ShouldDespawn()
        {
            return IsDespawning;
        }
        
        /// <summary>
        /// Calculate final speed using the same formula as ShaderScrollController
        /// finalSpeed = baseValue × parallaxMultiplier × gameSpeed
        /// </summary>
        protected float GetCalculatedSpeedX()
        {
            return moveSpeedX * parallaxMultiplier * Config.gameSpeed.Value;
        }
        
        protected float GetCalculatedSpeedY()
        {
            return moveSpeedY * parallaxMultiplier * Config.gameSpeed.Value;
        }
    }
}