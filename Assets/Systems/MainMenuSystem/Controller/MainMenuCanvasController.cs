using System;
using System.Net.Mime;
using Cysharp.Threading.Tasks;
using Systems.GameSystem;
using Systems.GameSystem.Config;
using Systems.GameSystem.Signals;
using Systems.InputSystem.Controller;
using Systems.MainMenuSystem.Model;
using Systems.MainMenuSystem.View;
using UniRx;
using UnityEngine.Device;
using UnityEngine.InputSystem;
using Zenject;

namespace Systems.MainMenuSystem.Controller
{
    [Serializable]
    public class MainMenuCanvasController : IDisposable
    {
        private readonly MainMenuCanvasModel _model;
        private readonly MainMenuCanvasView _view;
        private InputMaster _inputMaster;
        private SignalBus _signalBus;
        private GameConfig _gameConfig;

        private CompositeDisposable _disposable;

        public MainMenuCanvasController(MainMenuCanvasModel model, 
            MainMenuCanvasView view, 
            InputMaster inputMaster, 
            GameConfig gameConfig, 
            VirtualKeyboardController virtualKeyboardController, 
            SignalBus signalBus)
        {
            _model = model;
            _view = view;
            _inputMaster = inputMaster;
            _gameConfig = gameConfig;
            _signalBus = signalBus;
            _disposable = new CompositeDisposable();

            if (gameConfig.isRetrying.Value)
            {
                gameConfig.isRetrying.Value = false;
                TransitionToGameScreen().Forget();
            }
            else
            {
                PressButtonToStart();
            }

            Application.targetFrameRate = 60;
        }
        
        private Action<InputAction.CallbackContext> _pressButtonToStartAction;

        private async UniTaskVoid TransitionToGameScreen()
        {
            _inputMaster.Enable();
            _inputMaster.UiControl.Enable();
            _view.gameObject.SetActive(false);
            await UniTask.Delay(2000);
            _gameConfig.gamePhase.Value = GamePhase.GameScreen;
            _signalBus.Fire<GameScreenSignal>();
        }

        private void PressButtonToStart()
        {
            _inputMaster.Enable();
            _inputMaster.UiControl.Enable();
            _pressButtonToStartAction = _=> OnButtonPressed();
            _inputMaster.UiControl.Submit.performed += _pressButtonToStartAction;
            _inputMaster.UiControl.Click.performed += _pressButtonToStartAction;
        }

        private void OnButtonPressed()
        {
            _view.gameObject.SetActive(false);
            _signalBus.Fire<NameInputSignal>();
            _inputMaster.UiControl.Submit.performed -= _pressButtonToStartAction;
            _inputMaster.UiControl.Click.performed -= _pressButtonToStartAction;
        }
        
        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}