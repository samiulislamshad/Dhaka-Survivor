using System;
using System.Collections.Generic;
using Systems.GameSystem.Config;
using Systems.InputSystem.Model;
using Systems.InputSystem.View;
using UniRx;

namespace Systems.InputSystem.Controller
{
    [Serializable]
    public class VirtualKeyboardController : IDisposable
    {
        private GameConfig _config;
        private VirtualKeyboardView _view;
        private CompositeDisposable _disposable;

        private ReactiveProperty<string> _userName;
        private const int MaxLength = 20;
        private readonly List<string> _specialKeys = new() {"Submit", "Cancel", "Delete"};

        public VirtualKeyboardController(GameConfig config, VirtualKeyboardView view)
        {
            _config = config;
            _view = view;

            _userName = new ReactiveProperty<string>("");
            _disposable = new CompositeDisposable();
            
            SubscribeToProperties();
        }

        private void SubscribeToProperties()
        {
            _userName.Subscribe(value =>
            {
                _view.userNameText.text = value;
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

        public void ShowVirtualKeyboard()
        {
            _view.gameObject.SetActive(true);
        }

        public void HideVirtualKeyboard()
        {
            _view.gameObject.SetActive(false);
        }
        
        #endregion

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}