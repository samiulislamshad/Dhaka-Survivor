using System;
using UniRx;
using UnityEngine.InputSystem;
using Zenject;

namespace Systems.InputSystem.Service
{
    public enum InputDeviceType
    {
        KeyboardMouse,
        Gamepad,
        TouchScreen
    }

    [Serializable]
    public class InputDeviceDetector : IInitializable, IDisposable
    {
        public ReactiveProperty<InputDeviceType> CurrentDevice { get; private set; } 
            = new ReactiveProperty<InputDeviceType>(InputDeviceType.KeyboardMouse);

        public void Initialize()
        {
            UnityEngine.InputSystem.InputSystem.onActionChange += OnActionChange;
            
            if (Gamepad.current != null)
                CurrentDevice.Value = InputDeviceType.Gamepad;
            else if (Touchscreen.current != null)
                CurrentDevice.Value = InputDeviceType.TouchScreen;
        }

        private void OnActionChange(object obj, InputActionChange change)
        {
            if (change == InputActionChange.ActionPerformed)
            {
                var action = (InputAction)obj;
                var control = action.activeControl;

                if (control != null)
                {
                    if (control.device is Gamepad)
                    {
                        if (CurrentDevice.Value != InputDeviceType.Gamepad)
                            CurrentDevice.Value = InputDeviceType.Gamepad;
                    }
                    else if (control.device is Touchscreen)
                    {
                        if (CurrentDevice.Value != InputDeviceType.TouchScreen)
                            CurrentDevice.Value = InputDeviceType.TouchScreen;
                    }
                    else if (control.device is Keyboard || control.device is Mouse)
                    {
                        if (CurrentDevice.Value != InputDeviceType.KeyboardMouse)
                            CurrentDevice.Value = InputDeviceType.KeyboardMouse;
                    }
                }
            }
        }

        public void Dispose()
        {
            UnityEngine.InputSystem.InputSystem.onActionChange -= OnActionChange;
        }
    }
}
