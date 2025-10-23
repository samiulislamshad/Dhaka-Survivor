using System;
using Systems.GameSystem.Config;
using UniRx;
using UnityEngine;
using Zenject;

namespace Systems.ParallaxSystem.Handler
{
    public class ParallaxShaderEffectController : MonoBehaviour, IDisposable
    {
        private static readonly int ScrollSpeedX = Shader.PropertyToID("_ScrollSpeedX");
        private static readonly int SpeedMultiplier = Shader.PropertyToID("_SpeedMultiplier");
        private static readonly int Paused = Shader.PropertyToID("_IsPaused");
        private static readonly int ScrollSpeedY = Shader.PropertyToID("_ScrollSpeedY");
        private static readonly int ScrollDirection = Shader.PropertyToID("_ScrollDirection");
        [Inject] [SerializeField] private GameConfig config;

        [Header("Speed Controls")] [SerializeField]
        private float scrollSpeedX = -1.0f; // Negative for right-to-left

        [SerializeField] private float scrollSpeedY = 0.0f;
        [SerializeField] private float speedMultiplier = 1.0f;

        [Header("Playback Control")] [SerializeField]
        private bool isPaused = false;

        [Header("Alternative: Direction-based")] [SerializeField]
        private Vector2 scrollDirection = new(-1, 0);

        [SerializeField] private bool useDirectionMode = false;

        private Material _material;
        private SpriteRenderer _spriteRenderer;
        private CompositeDisposable _disposable;

        private void Awake()
        {
            scrollSpeedX = 0;
            speedMultiplier = 0;
            isPaused = true;
        }

        private void Start()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();

            // Create a unique material instance for this sprite
            _material = new Material(_spriteRenderer.material);
            _spriteRenderer.material = _material;
            _disposable = new CompositeDisposable();
            
            config.hasGameStarted.Subscribe(value =>
            {
                if (value)
                {
                    scrollSpeedX = 1;
                    speedMultiplier = 0.005f;
                    isPaused = false;
                }
                else
                {
                    scrollSpeedX = 0;
                    speedMultiplier = 0;
                    isPaused = true;
                }
            }).AddTo(_disposable);
            
            UpdateMaterialProperties();
        }

        private void Update()
        {
            UpdateMaterialProperties();
        }

        private void UpdateMaterialProperties()
        {
            if (_material == null) return;

            if (useDirectionMode)
            {
                // Use direction vector
                _material.SetVector(ScrollDirection, scrollDirection.normalized);
                _material.SetFloat(ScrollSpeedX, scrollDirection.magnitude);
                _material.SetFloat(ScrollSpeedY, 0);
            }
            else
            {
                // Use individual X/Y speeds
                _material.SetFloat(ScrollSpeedX, scrollSpeedX);
                _material.SetFloat(ScrollSpeedY, scrollSpeedY);
            }

            _material.SetFloat(SpeedMultiplier, speedMultiplier);
            _material.SetFloat(Paused, isPaused ? 1.0f : 0.0f);
        }

        // Public methods to change speed at runtime
        public void SetScrollSpeedX(float speed)
        {
            scrollSpeedX = speed;
        }

        public void SetScrollSpeedY(float speed)
        {
            scrollSpeedY = speed;
        }

        public void SetSpeedMultiplier(float multiplier)
        {
            speedMultiplier = multiplier;
        }

        public void SetScrollDirection(Vector2 direction)
        {
            scrollDirection = direction;
        }

        // Pause/Play controls
        public void Pause()
        {
            isPaused = true;
        }

        public void Play()
        {
            isPaused = false;
        }

        public void TogglePause()
        {
            isPaused = !isPaused;
            Debug.Log("Scrolling " + (isPaused ? "Paused" : "Playing"));
        }

        public bool IsPaused()
        {
            return isPaused;
        }

        private void OnDestroy()
        {
            // Clean up the material instance
            if (_material != null)
            {
                Destroy(_material);
            }
            Dispose();
        }

        #region Test

        [ContextMenu("DO Resume")]
        public void DoResume()
        {
        }

        #endregion

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}