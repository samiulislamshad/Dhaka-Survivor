using System;
using Systems.PlayerSystem.Signals;
using Systems.PlayerSystem.Signals.GameSignals;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace Systems.InputSystem.Handler
{
    public class PlayerInputHandler : MonoBehaviour
    {
        private InputMaster _inputControls;

        [Inject] private readonly SignalBus _signalBus;
        
        // private InputActionMap keyboardMap;
        // private InputActionMap gamepadMap;
        
        private Action<InputAction.CallbackContext> _startJumpInputAction;
        private Action<InputAction.CallbackContext> _stopJumpInputAction;
        
        private Action<InputAction.CallbackContext> _startCrouchInputAction;
        private Action<InputAction.CallbackContext> _stopCrouchInputAction;
        
        private Action<InputAction.CallbackContext> _togglePauseInputAction;
        
        
        #region Initializers

        private void Awake()
        {
            _inputControls = new InputMaster();
        }

        #region Subscribe and Unsubscribe

        private void SubscribeToActions()
        {
            _startCrouchInputAction = _ => StartCrouchInput();
            _stopCrouchInputAction = _ => StopCrouchInput();
            
            _startJumpInputAction = _ => StartJumpInput();
            _stopJumpInputAction = _ => StopJumpInput();
            
            _togglePauseInputAction = _ => TogglePause();
            
            _inputControls.PlayerControl.Crouch.performed += _startCrouchInputAction;
            _inputControls.PlayerControl.Crouch.canceled += _stopCrouchInputAction;
            
            _inputControls.PlayerControl.Jump.performed += _startJumpInputAction;
            _inputControls.PlayerControl.Jump.canceled += _stopJumpInputAction;
            
            _inputControls.PlayerControl.TogglePause.performed += _togglePauseInputAction;
            
            _signalBus.Subscribe<SwitchOnPlayerControlSignal>(SwitchOnPlayerControl);
            _signalBus.Subscribe<SwitchOffPlayerControlSignal>(SwitchOffPlayerControl);
            // _signalBus.Subscribe<PauseSignal>(DisablePlayerControl);
            // _signalBus.Subscribe<UnpauseSignal>(EnablePlayerControl);
        }

        private void UnSubscribeToActions()
        {
            _inputControls.PlayerControl.Crouch.performed += _startCrouchInputAction;
            _inputControls.PlayerControl.Crouch.canceled += _stopCrouchInputAction;
            
            _inputControls.PlayerControl.Jump.performed -= _startJumpInputAction;
            _inputControls.PlayerControl.Jump.canceled -= _stopJumpInputAction;
            
            _inputControls.PlayerControl.TogglePause.performed -= _togglePauseInputAction;
            
            _signalBus.Unsubscribe<SwitchOnPlayerControlSignal>(SwitchOnPlayerControl);
            _signalBus.Unsubscribe<SwitchOffPlayerControlSignal>(SwitchOffPlayerControl);
            // _signalBus.Unsubscribe<PauseSignal>(DisablePlayerControl);
            // _signalBus.Unsubscribe<UnpauseSignal>(EnablePlayerControl);
        }

        #endregion

        #region Enable and Disable

        private void OnEnable()
        {
            _inputControls.Enable();
            _inputControls.PlayerControl.Disable();
            _inputControls.UiControl.Enable();
            
            SubscribeToActions();
        }

        private void OnDisable()
        {
            UnSubscribeToActions();
            _inputControls.PlayerControl.Disable();
            _inputControls.UiControl.Disable();
            _inputControls.Disable();
        }

        #endregion

        #endregion

        private void SwitchOnPlayerControl()
        {
            _inputControls.PlayerControl.Enable();
        }
        private void SwitchOffPlayerControl()
        {
            _inputControls.PlayerControl.Disable();
        }

        #region Control Inputs
        
        private void StartJumpInput()
        {
            _signalBus.Fire<StartJumpInputSignal>();
        }

        private void StopJumpInput()
        {
            _signalBus.Fire<StopJumpInputSignal>();
        }
        
        private void StartCrouchInput()
        {
            _signalBus.Fire<StartCrouchInputSignal>();
        }
        
        private void StopCrouchInput()
        {
            _signalBus.Fire<StopCrouchInputSignal>();
        }
        
        private void TogglePause()
        {
            _signalBus.Fire<TogglePauseInputSignal>();
        }
        
        #endregion
        
        #region Pause and Unpause Controls

        private void DisablePlayerControl()
        {
            _inputControls.PlayerControl.Disable();
        }
        private void EnablePlayerControl()
        {
            _inputControls.PlayerControl.Enable();
        }

        #endregion
    }
}
