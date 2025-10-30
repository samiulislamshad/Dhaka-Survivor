using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Cysharp.Threading.Tasks;
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

        private ReactiveProperty<string> _userName;
        private const int MaxLength = 15;
        private const int MinLength = 3;
        private readonly List<string> _specialKeys = new() {"Submit", "Cancel", "Delete"};
        
        private EventSystem _eventSystem;
        
        private IDisposable _updateKeyboardFocus;

        public VirtualKeyboardController(GameConfig config, VirtualKeyboardView view, SignalBus signalBus)
        {
            _config = config;
            _view = view;
            _signalBus = signalBus;

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
            if(_eventSystem == null)
                _eventSystem = EventSystem.current;
            
            _view.gameObject.SetActive(true);
            _signalBus.Fire<SwitchOffPlayerControlSignal>();
            _eventSystem.SetSelectedGameObject(_view.allButtons[0].gameObject);
            _updateKeyboardFocus = Observable.EveryUpdate().Where(_ => 
                _view.gameObject.activeInHierarchy && 
                (_eventSystem == null || _eventSystem.currentSelectedGameObject == null))
                .Subscribe(_ =>
                {
                    KeepFocusOnKeyboard();
                });
        }

        public void HideVirtualKeyboard()
        {
            _view.gameObject.SetActive(false);
            _updateKeyboardFocus.Dispose();
            _signalBus.Fire<GameScreenSignal>();
        }

        #region Utility

        private void KeepFocusOnKeyboard()
        {
            if(!_view.gameObject.activeSelf) return;
            
            if (_eventSystem.currentSelectedGameObject == null)
            {
                _eventSystem.SetSelectedGameObject(_view.allButtons[0].gameObject);
                return;
            }

            foreach (var button in _view.allButtons)
            {
                if(button.gameObject == _eventSystem.currentSelectedGameObject) return;
                _eventSystem.SetSelectedGameObject(_view.allButtons[0].gameObject);
            }
        }

        #endregion

        public void Dispose()
        {
            UnsubscribeFromSignals();
            _disposable.Dispose();
        }
    }
}