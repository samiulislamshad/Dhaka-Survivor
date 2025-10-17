using Systems.PlayerSystem.Signals;
using UnityEngine;
using Zenject;

namespace Systems.PlayerSystem.Controller
{
    public class PlayerController : MonoBehaviour
    {
        private SignalBus _signalBus;

        [Header("Jump Settings")] 
        [SerializeField] private float jumpForce = 10f;

        [SerializeField] private float normalGravity = 5f;
        [SerializeField] private float maxJumpTime = 0.3f;
        [SerializeField] private float jumpMultiplier = 1.5f;

        [Header("Ground Check")] 
        [SerializeField] private Transform groundCheck;

        [SerializeField] private float groundCheckRadius = 0.2f;
        [SerializeField] private LayerMask groundLayer;

        [Header("Crouch Settings")] 
        [SerializeField] private float crouchSpeed = 2.5f;

        [SerializeField] private float midAirCrouchForce = 10f;
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
            else isJumping = false;
        }

        private void OnJumpPerformed(StartJumpInputSignal signal)
        {
            if(!isGrounded || isCrouching) return;
            isJumping = true;
            _jumpHeld = true;
            jumpTimeCounter = maxJumpTime;
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpForce);
        }

        private void OnJumpCanceled(StopJumpInputSignal signal)
        {
            _jumpHeld = false;
            isJumping = false;
        }

        #endregion

        #region Crouch

        private void OnCrouchPerformed(StartCrouchInputSignal signal)
        {
            if (isCrouching) return;
            if(!isGrounded)
                _rb.gravityScale = midAirCrouchForce;
            else
            {
                _rb.gravityScale = normalGravity;
                Crouch();
            }
        }

        private void OnCrouchCanceled(StopCrouchInputSignal signal)
        {
            _rb.gravityScale = normalGravity;
            if(!isCrouching) return;
            StandUp();
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