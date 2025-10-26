using Systems.AudioSystem.Handler;
using Systems.GameSystem.Config;
using Systems.PauseSystem.Signals;
using Systems.PlayerSystem.Signals;
using Systems.PlayerSystem.Signals.GameSignals;
using UnityEngine;
using Zenject;

namespace Systems.PlayerSystem.Controller
{
    public class PlayerController : MonoBehaviour
    {
        private SignalBus _signalBus;
        [Inject] private GameConfig _gameConfig;

        [SerializeField] private OneShotPlayer jumpSfx;
        [SerializeField] private OneShotPlayer gameOverSfx;

        [Header("Jump Settings")] [SerializeField]
        private float jumpForceWhenHeld = 32f;

        [SerializeField] private float jumpForceWhenReleased = 25f;
        [SerializeField] private float jumpForce = 25f; // Higher initial burst

        [SerializeField] private float jumpGravityWhenHeld = 8f;
        [SerializeField] private float jumpGravityWhenReleased = 10f;
        [SerializeField] private float jumpGravity = 10f; // Gravity while ascending
        [SerializeField] private float fallGravity = 20f; // Much stronger gravity when falling
        [SerializeField] private float maxJumpTime = 0.01f; // Shorter for snappier feel
        [SerializeField] private float jumpMultiplier = 1.5f;

        // [Header("Ground Check")] [SerializeField]
        // private Transform groundCheck;

        [SerializeField] private float groundCheckRadius = 0.2f;
        [SerializeField] private LayerMask groundLayer;

        [Header("Crouch Settings")] [SerializeField]
        private float crouchSpeed = 2.5f;

        [SerializeField] private float fastFallGravity = 50f; // Extra fast fall when crouching
        // [SerializeField] private float crouchScale = 0.5f;

        [Header("Animation Settings")] [SerializeField]
        private Animator animator; // Reference to the Animator component

        [SerializeField] private float landingThreshold = -10f; // Minimum velocity for hard landing animation

        [SerializeField] private float idleDelay = 0.1f; // Delay before transitioning from landing to idle/run

        // ========== NEW: ANIMATION VARIABLES ==========
        private bool _wasGrounded; // Tracks previous frame's grounded state for landing detection
        private PlayerAnimState _currentAnimState; // Current animation state
        private float _landingTimer; // Timer to hold landing animation before transitioning

        private static readonly int AnimStateHash = Animator.StringToHash("AnimState");
        // ==============================================

        #region Animation Enum

        // ========== NEW: ANIMATION STATE ENUM ==========
        /// <summary>
        /// Enum representing all possible player animation states
        /// Values correspond to Animator Controller integer parameter
        /// </summary>
        public enum PlayerAnimState
        {
            Idle = 0, // Standing still on ground
            Run = 1, // Moving on ground
            Jump = 2, // Ascending in air
            Fall = 3, // Descending in air
            LandingStart = 4, // Hard landing animation
            Land = 5 // Soft landing animation
        }

        // ===============================================

        #endregion

        [SerializeField] private bool isDead;

        private Rigidbody2D _rb;
        private BoxCollider2D _col;

        [SerializeField] private bool isGrounded;
        [SerializeField] private bool isJumping;
        [SerializeField] private bool isCrouching;
        [SerializeField] private float jumpTimeCounter;

        private bool _jumpHeld;

        #region Initializers

        [Inject]
        private void InjectDiReferences(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        private void Start()
        {
            _rb = GetComponent<Rigidbody2D>();
            _col = GetComponent<BoxCollider2D>();
            isDead = false;

            // if (animator == null)
            //     animator = GetComponent<Animator>();
            _wasGrounded = true; // Assume player starts on ground
            _landingTimer = 0f; // No landing animation at start

            SubscribeToSignals();
            // SetAnimationState(PlayerAnimState.Idle);
            animator.Play("idle");
        }

        #endregion

        #region Subscribe and Unsubscribe

        private void SubscribeToSignals()
        {
            _signalBus.Subscribe<StartJumpInputSignal>(OnJumpPerformed);
            _signalBus.Subscribe<StopJumpInputSignal>(OnJumpCanceled);
            _signalBus.Subscribe<StartCrouchInputSignal>(OnCrouchPerformed);
            _signalBus.Subscribe<StopCrouchInputSignal>(OnCrouchCanceled);
            _signalBus.Subscribe<ContactWithEnemySignal>(Death);
            _signalBus.Subscribe<PlayerSpecialJumpSignal>(PerformSecondJump);
        }

        private void UnsubscribeFromActions()
        {
            _signalBus.Unsubscribe<StartJumpInputSignal>(OnJumpPerformed);
            _signalBus.Unsubscribe<StopJumpInputSignal>(OnJumpCanceled);
            _signalBus.Unsubscribe<StartCrouchInputSignal>(OnCrouchPerformed);
            _signalBus.Unsubscribe<StopCrouchInputSignal>(OnCrouchCanceled);
            _signalBus.Unsubscribe<ContactWithEnemySignal>(Death);
            _signalBus.Unsubscribe<PlayerSpecialJumpSignal>(PerformSecondJump);
        }

        #endregion

        private void Update()
        {
            if (isDead) return;
            HandleJump();
            HandleGravity();
            UpdateAnimation();
        }

        #region Animation
        
        private void UpdateAnimation()
        {
            if (isDead) return;
            
            if (_landingTimer > 0)
            {
                _landingTimer -= Time.deltaTime;
                return; 
            }
            
            if (!_wasGrounded && isGrounded)
            {
                var landingVelocity = _rb.linearVelocity.y;
                
                if (landingVelocity <= landingThreshold)
                {
                 
                    // SetAnimationState(PlayerAnimState.LandingStart);
                    animator.Play("landingStart");
                    _landingTimer = idleDelay;
                }
                else
                {
                  
                    // SetAnimationState(PlayerAnimState.Land);
                    // animator.Play("land");
                    _landingTimer = idleDelay * 0.5f; 
                }
            }
            // ===== AIR STATES =====
            // Player is not grounded - check vertical movement
            else if (!isGrounded)
            {
                if (_rb.linearVelocity.y > 0.1f)
                {
                    // Moving upward - jumping
                    // SetAnimationState(PlayerAnimState.Jump);
                    animator.Play("jump");
                }
                else if (_rb.linearVelocity.y < -0.1f)
                {
                    // Moving downward - falling
                    // SetAnimationState(PlayerAnimState.Fall);
                    animator.Play("fall");
                }
            }
            // ===== GROUNDED STATES =====
            // Player is on ground - check horizontal movement
            else if (isGrounded)
            {
                // Check for horizontal movement (adjust threshold based on your game)
                if (_gameConfig.hasGameStarted.Value && !isDead)
                {
                    // Player is moving - play run animation
                    // SetAnimationState(PlayerAnimState.Run);
                    animator.Play("run");
                }
                else
                {
                    // Player is stationary - play idle animation
                    // SetAnimationState(PlayerAnimState.Idle);
                    animator.Play("idle");
                }
            }

            // Store current grounded state for next frame's landing detection
            _wasGrounded = isGrounded;
        }
        

        #endregion

        #region Ground Check

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (IsGroundLayer(collision.gameObject))
                isGrounded = true;
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (IsGroundLayer(collision.gameObject))
                isGrounded = true;
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (IsGroundLayer(collision.gameObject))
                isGrounded = false;
        }

        private bool IsGroundLayer(GameObject obj)
        {
            return ((1 << obj.layer) & groundLayer) != 0;
        }

        #endregion

        #region Jump

        private void HandleJump()
        {
            if (!_jumpHeld || !isJumping) return;

            if (jumpTimeCounter > 0)
            {
                jumpSfx.PlayAudio();
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpForce * jumpMultiplier);
                jumpTimeCounter -= Time.deltaTime;
            }
            else
            {
                isJumping = false;
            }
        }

        private void HandleGravity()
        {
            if (isGrounded)
            {
                _rb.gravityScale = jumpGravity; // Reset to normal
                return;
            }

            // Fast fall when crouching in air
            if (isCrouching)
            {
                _rb.gravityScale = fastFallGravity;
            }
            // Strong gravity when falling (like Chrome dino)
            else if (_rb.linearVelocity.y < 0)
            {
                _rb.gravityScale = fallGravity;
            }
            // Light gravity while ascending
            else
            {
                _rb.gravityScale = jumpGravity;
            }
        }

        private void OnJumpPerformed(StartJumpInputSignal signal)
        {
            if (!isGrounded || isCrouching) return;

            jumpForce = jumpForceWhenHeld;
            jumpGravity = jumpGravityWhenHeld;
            isJumping = true;
            _jumpHeld = true;
            jumpTimeCounter = maxJumpTime;
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpForce);
            
            // SetAnimationState(PlayerAnimState.Jump);
            animator.Play("jump");
            _landingTimer = 0f; // Clear landing timer to allow immediate animation change
        }

        private void OnJumpCanceled(StopJumpInputSignal signal)
        {
            jumpForce = jumpForceWhenReleased;
            jumpGravity = jumpForceWhenReleased;
            _jumpHeld = false;
            isJumping = false;
        }

        #endregion

        #region Second Jump

        [Header("Second Jump Settings")]
        [SerializeField] private float secondJumpForce = 35f; // Higher for satisfying bounce
        
        private void PerformSecondJump()
        {
            if (isDead) return;
            if (isGrounded) return;
            
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, secondJumpForce);
            isJumping = true;
            _jumpHeld = true;
            jumpTimeCounter = maxJumpTime * 0.5f;
            animator.Play("jump");
            _landingTimer = 0f;
        }

        #endregion

        #region Crouch

        private void OnCrouchPerformed(StartCrouchInputSignal signal)
        {
            if (isCrouching) return;
            if (!isGrounded)
                isCrouching = true; // Set crouching state in mid-air
        }

        private void OnCrouchCanceled(StopCrouchInputSignal signal)
        {
            if (!isCrouching) return;
            isCrouching = false; // Just reset the flag in mid-air
        }

        #endregion

        #region Death

        private void Death()
        {
            isDead = true;
            _signalBus.Fire<PlayerDeadSignal>();
            _signalBus.Fire<PauseSignal>();
            _gameConfig.hasGameStarted.Value = false;
            _gameConfig.hasTimerStarted.Value = false;
            _rb.linearVelocity = Vector2.zero;
            _rb.gravityScale = 0;
            gameOverSfx.PlayAudio();
        }

        #endregion

        private void OnDestroy()
        {
            UnsubscribeFromActions();
        }
    }
}