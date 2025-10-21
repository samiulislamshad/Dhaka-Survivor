using Systems.GameSystem.Config;
using UnityEngine;
using Zenject;

namespace Systems.ParallaxSystem.Handler
{
    public class ParallaxEffectController : MonoBehaviour
    {
        [Inject] [SerializeField] private GameConfig config;

        [Header("Scroll Speed Settings")] [SerializeField]
        private float scrollSpeedX = 0.5f;

        [SerializeField] private float scrollSpeedY = 0f;

        [Header("Parallax Settings")] [SerializeField]
        private float parallaxMultiplier = 1f;

        [SerializeField] private float baseSpeed = 1f;

        [Header("Runtime Controls")] [SerializeField]
        private bool canPause = true;

        private bool isPaused = false;

        private Material material;
        private SpriteRenderer spriteRenderer;

        // Shader property names
        private static readonly int ScrollOffsetProperty = Shader.PropertyToID("_ScrollOffset");

        // Accumulated scroll offset
        private Vector2 scrollOffset = Vector2.zero;

        void Start()
        {
            // Get the SpriteRenderer component
            spriteRenderer = GetComponent<SpriteRenderer>();

            if (spriteRenderer == null)
            {
                Debug.LogError("ShaderScrollController requires a SpriteRenderer component!");
                enabled = false;
                return;
            }

            // Create a unique material instance for this object
            material = spriteRenderer.material;

            // Set initial scroll speeds
            UpdateShaderSpeed();
        }

        void Update()
        {
            // // Optional: Press Space to toggle pause
            // if (canPause && Input.GetKeyDown(KeyCode.Space))
            // {
            //     TogglePause();
            // }

            // Accumulate scroll offset each frame
            if (!isPaused)
            {
                float finalSpeedX = scrollSpeedX * parallaxMultiplier * baseSpeed;
                float finalSpeedY = scrollSpeedY * parallaxMultiplier * baseSpeed;

                scrollOffset.x += finalSpeedX * Time.deltaTime;
                scrollOffset.y += finalSpeedY * Time.deltaTime;

                UpdateShaderOffset();
            }
        }

        void UpdateShaderOffset()
        {
            if (material != null)
            {
                material.SetVector(ScrollOffsetProperty, scrollOffset);
            }
        }

        void UpdateShaderSpeed()
        {
            // This method is kept for compatibility but offset is now updated in Update()
        }

        // Public methods to control speed at runtime
        public void SetScrollSpeed(float speedX, float speedY = 0f)
        {
            scrollSpeedX = speedX;
            scrollSpeedY = speedY;
            UpdateShaderSpeed();
        }

        public void SetScrollSpeedX(float speed)
        {
            scrollSpeedX = speed;
            UpdateShaderSpeed();
        }

        public void SetScrollSpeedY(float speed)
        {
            scrollSpeedY = speed;
            UpdateShaderSpeed();
        }

        public void SetParallaxMultiplier(float multiplier)
        {
            parallaxMultiplier = multiplier;
            UpdateShaderSpeed();
        }

        public void SetBaseSpeed(float speed)
        {
            baseSpeed = speed;
            UpdateShaderSpeed();
        }

        public void TogglePause()
        {
            isPaused = !isPaused;
            UpdateShaderSpeed();
        }

        public void Pause()
        {
            isPaused = true;
        }

        public void Resume()
        {
            isPaused = false;
        }

        public void ResetScrollOffset()
        {
            scrollOffset = Vector2.zero;
            UpdateShaderOffset();
        }

        void OnDestroy()
        {
            // Clean up the material instance
            if (material != null)
            {
                Destroy(material);
            }
        }

        #region Test

        [ContextMenu("DO Resume")]
        public void DoResume()
        {
            
        }

        #endregion
    }
}