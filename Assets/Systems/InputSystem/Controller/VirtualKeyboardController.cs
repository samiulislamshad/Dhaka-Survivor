using System;
using System.Collections.Generic;
using Systems.GameSystem.Config;
using Systems.InputSystem.Model;
using Systems.InputSystem.View;
using Systems.PlayerSystem.Signals.GameSignals;
using UniRx;
using UnityEngine.EventSystems;
using Zenject;

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
        private const int MaxLength = 20;
        private const int MinLength = 3;
        private readonly List<string> _specialKeys = new() {"Submit", "Cancel", "Delete"};

        public VirtualKeyboardController(GameConfig config, VirtualKeyboardView view, SignalBus signalBus)
        {
            _config = config;
            _view = view;
            _signalBus = signalBus;

            _userName = new ReactiveProperty<string>("");
            _disposable = new CompositeDisposable();
            
            SubscribeToProperties();
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
                OnSubmit();
            }).AddTo(_disposable);
            
            _view.cancelButton.OnClickAsObservable().Subscribe(_ =>
            {
                OnCancel();
            }).AddTo(_disposable);
            
            _view.deleteButton.OnClickAsObservable().Subscribe(_ =>
            {
                OnDelete();
            }).AddTo(_disposable);
            
            _view.InitializeAlphanumericButtons(_specialKeys, OnLetterPressed);
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

        private void OnSubmit()
        {
            if(_userName.Value.Length <= 0) return;
            var dateTime = DateTime.Now;
            var userData = new UserData
            {
                userName = _userName.Value,
                date = dateTime.ToShortDateString(),
                time = dateTime.ToShortTimeString()
            };

            _config.currentUserData = userData;
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
            _view.gameObject.SetActive(true);
            _signalBus.Fire<SwitchOffPlayerControlSignal>();
            EventSystem.current.SetSelectedGameObject(_view.deleteButton.gameObject);
        }

        public void HideVirtualKeyboard()
        {
            _view.gameObject.SetActive(false);
            _signalBus.Fire<SwitchOnPlayerControlSignal>();
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}