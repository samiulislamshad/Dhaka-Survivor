using System;
using Systems.GameSystem.Config;
using Systems.GameSystem.View;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;
using Zenject;

namespace Systems.GameSystem.Service
{
    [Serializable]
    public class GameInactivityDetector : ITickable, IDisposable
    {
        private GameConfig _gameConfig;
        private InactivityWarningUI _inactivityWarningUI;
        private CompositeDisposable _disposable;
        
        private float _inactivityTime = 30f;
        private float _inactivityWarningTime = 5f;
        private string _mainMenuSceneName = "Game";
    
        private float _timeSinceLastInput;
        private bool _warningShown;
        
        public GameInactivityDetector(GameConfig gameConfig, 
            InactivityWarningUI inactivityWarningUI)
        {
            _gameConfig = gameConfig;
            _inactivityWarningUI = inactivityWarningUI;
            _timeSinceLastInput = 0f;

            _disposable = new CompositeDisposable();
            _inactivityWarningUI.gameObject.SetActive(false);
        }


        public void Tick()
        {
            if(_gameConfig.gamePhase.Value == GamePhase.MainMenuScreen) return;
            if (CheckForGamepadInput())
            {
                ResetTimer();
                if(_inactivityWarningUI.gameObject.activeSelf)
                    _inactivityWarningUI.gameObject.SetActive(false);
                return;
            }

            _timeSinceLastInput += Time.unscaledDeltaTime;
        
            if (!_warningShown && _timeSinceLastInput >= _inactivityTime - _inactivityWarningTime)
            {
                _warningShown = true;
                _inactivityWarningUI.gameObject.SetActive(true);
            }
        
            if (_timeSinceLastInput >= _inactivityTime)
            {
                SceneManager.LoadScene(_mainMenuSceneName);
            }
        }

        private static bool CheckForGamepadInput()
        {
            if (Gamepad.current == null) return false;

            foreach (var control in Gamepad.current.allControls)
            {
                if (control is ButtonControl { isPressed: true })
                    return true;
                if (control is StickControl stick && stick.ReadValue().magnitude > 0.1f)
                    return true;
                if (control is AxisControl axis && Mathf.Abs(axis.ReadValue()) > 0.1f)
                    return true;
            }

            return false;
        }

        private void ResetTimer()
        {
            _timeSinceLastInput = 0f;
            _warningShown = false;
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}