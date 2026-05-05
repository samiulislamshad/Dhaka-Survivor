using System;
using System.Collections.Generic;
using System.Globalization;
using Systems.InputSystem.Service;
using UnityEngine.InputSystem;
using System.Linq;
using Cysharp.Threading.Tasks;
using Systems.GameSystem;
using Systems.GameSystem.Config;
using Systems.GameSystem.Signals;
using Systems.InputSystem.Model;
using Systems.InputSystem.View;
using Systems.PlayerSystem.Signals.GameSignals;
using UniRx;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;
using Random = UnityEngine.Random;

namespace Systems.InputSystem.Controller
{
    [Serializable]
    public class VirtualKeyboardController : IDisposable
    {
        private GameConfig _config;
        private VirtualKeyboardView _view;
        private SignalBus _signalBus;
        private CompositeDisposable _disposable;
        private GameConfig _gameConfig;
        private InputDeviceDetector _inputDeviceDetector;

        private ReactiveProperty<string> _userName;
        private const int MaxLength = 15;
        private const int MinLength = 3;
        private readonly List<string> _specialKeys = new() {"Submit", "Cancel", "Delete"};
        
        private EventSystem _eventSystem;
        
        private IDisposable _updateKeyboardFocus;

        public VirtualKeyboardController(GameConfig config, VirtualKeyboardView view, SignalBus signalBus, GameConfig gameConfig, InputDeviceDetector inputDeviceDetector)
        {
            _config = config;
            _view = view;
            _signalBus = signalBus;
            _gameConfig = gameConfig;
            _inputDeviceDetector = inputDeviceDetector;

            _userName = new ReactiveProperty<string>("");
            _disposable = new CompositeDisposable();
            _eventSystem = EventSystem.current;
            
            SubscribeToProperties();
            SubscribeToSignals();
            
            view.gameObject.SetActive(false);
        }

        private void SubscribeToProperties()
        {
            _userName.Subscribe(value =>
            {
                _view.userNameText.text = value;
                _view.submitButton.interactable = _userName.Value.Length >= MinLength;
            }).AddTo(_disposable);

            _view.submitButton.OnClickAsObservable().Subscribe(_ =>
            {
                OnSubmit().Forget();
            }).AddTo(_disposable);
            
            _view.deleteButton.OnClickAsObservable().Subscribe(_ =>
            {
                OnDelete();
            }).AddTo(_disposable);
            
            _view.InitializeAlphanumericButtons(_specialKeys, OnLetterPressed);

            _inputDeviceDetector.CurrentDevice.Subscribe(deviceType =>
            {
                UpdateKeyboardVisibility(deviceType);
            }).AddTo(_disposable);
        }

        private void UpdateKeyboardVisibility(InputDeviceType deviceType)
        {
            if (!_view.gameObject.activeInHierarchy) return;
            
            bool useGamepad = deviceType == InputDeviceType.Gamepad;
            _view.ToggleVirtualKeyboardKeys(useGamepad);

            if (!useGamepad && _eventSystem.currentSelectedGameObject != null)
            {
                _eventSystem.SetSelectedGameObject(null);
            }
        }

        private void SubscribeToSignals()
        {
            _signalBus.Subscribe<NameInputSignal>(ShowVirtualKeyboard);
        }

        private void UnsubscribeFromSignals()
        {
            _signalBus.Unsubscribe<NameInputSignal>(ShowVirtualKeyboard);
        }

        #region Buttons
        
        private void OnLetterPressed(string letter)
        {
            if (_userName.Value.Length < MaxLength)
                _userName.Value += letter;
        }
        
        private void OnDelete()
        {
            if (_userName.Value.Length <= 0) return;
            _userName.Value = _userName.Value[..^1];
        }

        private async UniTaskVoid OnSubmit()
        {
            if(_userName.Value.Length <= 0) return;
            var dateTime = DateTime.Now;
            var userData = new UserData(Random.Range(0,50), _userName.Value, 0, Guid.NewGuid().ToString())
            {
                date = dateTime.Date.ToString(CultureInfo.InvariantCulture),
                time = dateTime.Hour.ToString().PadLeft(2, '0'),
            };
            
            _config.currentUserData = userData;
            await UniTask.Delay(100);
            HideVirtualKeyboard();
        }

        private void OnCancel()
        {
            _userName.Value = "";
            HideVirtualKeyboard();
        }
        
        #endregion

        public void ShowVirtualKeyboard()
        {
            _gameConfig.gamePhase.Value = GamePhase.NameInputScreen;
            if(_eventSystem == null)
                _eventSystem = EventSystem.current;
            
            _view.gameObject.SetActive(true);
            UpdateKeyboardVisibility(_inputDeviceDetector.CurrentDevice.Value);
            _signalBus.Fire<SwitchOffPlayerControlSignal>();
            
            if (Keyboard.current != null)
                Keyboard.current.onTextInput += OnTextInput;
                
            FocusKey().Forget();
        }

        private async UniTaskVoid FocusKey()
        {
            await UniTask.Delay(1000);
            
            if (_inputDeviceDetector.CurrentDevice.Value == InputDeviceType.Gamepad)
                _eventSystem.SetSelectedGameObject(_view.allButtons[0].gameObject);
                
            _updateKeyboardFocus = Observable.EveryUpdate().Where(_ => 
                    _view.gameObject.activeInHierarchy)
                .Subscribe(_ =>
                {
                    HandleKeyboardInput();
                    KeepFocusOnKeyboard();
                });
        }

        public void HideVirtualKeyboard()
        {
            _view.gameObject.SetActive(false);
            if (Keyboard.current != null)
                Keyboard.current.onTextInput -= OnTextInput;
            _updateKeyboardFocus.Dispose();
            _signalBus.Fire<GameScreenSignal>();
        }

        #region Utility

        private void OnTextInput(char c)
        {
            if (_inputDeviceDetector.CurrentDevice.Value != InputDeviceType.KeyboardMouse) return;

            if (char.IsLetterOrDigit(c) || char.IsPunctuation(c) || c == ' ')
            {
                OnLetterPressed(c.ToString().ToUpper());
            }
        }

        private void HandleKeyboardInput()
        {
            if (_inputDeviceDetector.CurrentDevice.Value != InputDeviceType.KeyboardMouse || Keyboard.current == null) return;

            if (Keyboard.current.backspaceKey.wasPressedThisFrame)
            {
                OnDelete();
            }
            if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame)
            {
                OnSubmit().Forget();
            }
        }

        private void KeepFocusOnKeyboard()
        {
            if(!_view.gameObject.activeSelf) return;
            
            if (_inputDeviceDetector.CurrentDevice.Value == InputDeviceType.KeyboardMouse)
            {
                if (_eventSystem.currentSelectedGameObject != null)
                {
                    _eventSystem.SetSelectedGameObject(null);
                }
                return;
            }

            if (_eventSystem.currentSelectedGameObject == null)
            {
                _eventSystem.SetSelectedGameObject(_view.allButtons[0].gameObject);
                return;
            }

            foreach (var button in _view.allButtons)
            {
                if(button.gameObject == _eventSystem.currentSelectedGameObject) return;
            }
            _eventSystem.SetSelectedGameObject(_view.allButtons[0].gameObject);
        }

        #endregion

        public void Dispose()
        {
            UnsubscribeFromSignals();
            _disposable.Dispose();
        }
    }
}