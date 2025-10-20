using UnityEngine;

namespace Systems.LevelSystem.Handler
{
public class ParallaxEffectController : MonoBehaviour
{
    [Header("Scroll Speed Settings")]
    [SerializeField] private float scrollSpeedX = 0.5f;
    [SerializeField] private float scrollSpeedY = 0f;
    
    [Header("Parallax Settings")]
    [SerializeField] private float parallaxMultiplier = 1f;
    [SerializeField] private float baseSpeed = 1f;
    
    [Header("Runtime Controls")]
    [SerializeField] private bool canPause = true;
    private bool _isPaused = false;
    
    private Material _material;
    private SpriteRenderer _spriteRenderer;
    
    // Shader property names
    private static readonly int ScrollSpeedXProperty = Shader.PropertyToID("_ScrollSpeedX");
    private static readonly int ScrollSpeedYProperty = Shader.PropertyToID("_ScrollSpeedY");

    private void Start()
    {
        // Get the SpriteRenderer component
        _spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (_spriteRenderer == null)
        {
            Debug.LogError("ShaderScrollController requires a SpriteRenderer component!");
            enabled = false;
            return;
        }
        
        // Create a unique material instance for this object
        _material = _spriteRenderer.material;
        
        // Set initial scroll speeds
        UpdateShaderSpeed();
    }

    #region For Testing

    [SerializeField] private float floatValue = 0f;

    [ContextMenu("Set Parallax Multiplier")]
    public void SetParallaxMultiplierWithValue()
    {
        SetParallaxMultiplier(floatValue);
    }
    
    [ContextMenu("Set Base Speed")]
    public void SetBaseSpeed()
    {
        SetBaseSpeed(floatValue);
    }
    
    [ContextMenu("Pause")]
    public void PauseShader()
    {
        Pause();
    }
    
    [ContextMenu("Resume")]
    public void ResumeShader()
    {
        Resume();
    }
    
    #endregion

    private void UpdateShaderSpeed()
    {
        if (_material != null)
        {
            var finalSpeedX = _isPaused ? 0f : scrollSpeedX * parallaxMultiplier * baseSpeed;
            var finalSpeedY = _isPaused ? 0f : scrollSpeedY * parallaxMultiplier * baseSpeed;
            
            _material.SetFloat(ScrollSpeedXProperty, finalSpeedX);
            _material.SetFloat(ScrollSpeedYProperty, finalSpeedY);
        }
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
        _isPaused = !_isPaused;
        UpdateShaderSpeed();
    }

    public void Pause()
    {
        _isPaused = true;
        UpdateShaderSpeed();
    }

    public void Resume()
    {
        _isPaused = false;
        UpdateShaderSpeed();
    }

    // Allow changes in Inspector to update in real-time
    private void OnValidate()
    {
        if (Application.isPlaying && _material != null)
        {
            UpdateShaderSpeed();
        }
    }

    private void OnDestroy()
    {
        // Clean up the material instance
        if (_material != null)
        {
            Destroy(_material);
        }
    }
}}