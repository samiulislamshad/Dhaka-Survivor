using Systems.PlayerSystem.Signals;
using UnityEngine;
using Zenject;

namespace Systems.PlayerSystem.Controller
{
    public class PlayerController : MonoBehaviour
    {
        private SignalBus _signalBus;

        [Header("Jump Settings")] 
        [SerializeField] private float jumpForceWhenHeld = 32f;
        [SerializeField] private float jumpForceWhenReleased = 25f;
        [SerializeField] private float jumpForce = 25f; // Higher initial burst
        
        [SerializeField] private float jumpGravityWhenHeld = 8f;
        [SerializeField] private float jumpGravityWhenReleased = 10f;
        [SerializeField] private float jumpGravity = 10f; // Gravity while ascending
        [SerializeField] private float fallGravity = 20f; // Much stronger gravity when falling
        [SerializeField] private float maxJumpTime = 0.01f; // Shorter for snappier feel
        [SerializeField] private float jumpMultiplier = 1.5f;

        [Header("Ground Check")] [SerializeField]
        private Transform groundCheck;

        [SerializeField] private float groundCheckRadius = 0.2f;
        [SerializeField] private LayerMask groundLayer;

        [Header("Crouch Settings")] 
        [SerializeField] private float crouchSpeed = 2.5f;

        [SerializeField] private float fastFallGravity = 50f; // Extra fast fall when crouching
        [SerializeField] private float crouchScale = 0.5f;

        private Rigidbody2D _rb;
        private BoxCollider2D _col;
        private Vector3 _originalScale;
        private float _originalColliderHeight;
        private float _originalColliderOffsetY;

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
            _originalScale = transform.localScale;
            _originalColliderHeight = _col.size.y;
            _originalColliderOffsetY = _col.offset.y;

            SubscribeToActions();
        }

        #endregion

        #region Subscribe and Unsubscribe

        private void SubscribeToActions()
        {
            _signalBus.Subscribe<StartJumpInputSignal>(OnJumpPerformed);
            _signalBus.Subscribe<StopJumpInputSignal>(OnJumpCanceled);
            _signalBus.Subscribe<StartCrouchInputSignal>(OnCrouchPerformed);
            _signalBus.Subscribe<StopCrouchInputSignal>(OnCrouchCanceled);
        }

        private void UnsubscribeFromActions()
        {
            _signalBus.Unsubscribe<StartJumpInputSignal>(OnJumpPerformed);
            _signalBus.Unsubscribe<StopJumpInputSignal>(OnJumpCanceled);
            _signalBus.Unsubscribe<StartCrouchInputSignal>(OnCrouchPerformed);
            _signalBus.Unsubscribe<StopCrouchInputSignal>(OnCrouchCanceled);
        }

        #endregion

        private void Update()
        {
            HandleJump();
            HandleGravity();
        }

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
            jumpGravity =  jumpGravityWhenHeld;
            isJumping = true;
            _jumpHeld = true;
            jumpTimeCounter = maxJumpTime;
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpForce);
        }

        private void OnJumpCanceled(StopJumpInputSignal signal)
        {
            jumpForce = jumpForceWhenReleased;
            jumpGravity = jumpForceWhenReleased;
            _jumpHeld = false;
            isJumping = false;
        }

        #endregion

        #region Crouch

        private void OnCrouchPerformed(StartCrouchInputSignal signal)
        {
            if (isCrouching) return;
            if (!isGrounded)
            {
                isCrouching = true; // Set crouching state in mid-air
            }
            else
            {
                Crouch();
            }
        }

        private void OnCrouchCanceled(StopCrouchInputSignal signal)
        {
            if (!isCrouching) return;

            if (isGrounded)
                StandUp();
            else
                isCrouching = false; // Just reset the flag in mid-air
        }

        private void Crouch()
        {
            isCrouching = true;

            transform.localScale = new Vector3(
                _originalScale.x,
                _originalScale.y * crouchScale,
                _originalScale.z
            );

            _col.size = new Vector2(_col.size.x, _originalColliderHeight * crouchScale);
            _col.offset = new Vector2(_col.offset.x, _originalColliderOffsetY * crouchScale);
        }

        private void StandUp()
        {
            isCrouching = false;
            transform.localScale = _originalScale;
            _col.size = new Vector2(_col.size.x, _originalColliderHeight);
            _col.offset = new Vector2(_col.offset.x, _originalColliderOffsetY);
        }

        #endregion

        private void OnDestroy()
        {
            UnsubscribeFromActions();
        }
    }
}