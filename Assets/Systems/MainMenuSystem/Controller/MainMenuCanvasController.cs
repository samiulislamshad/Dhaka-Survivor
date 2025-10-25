using System;
using Cysharp.Threading.Tasks;
using Systems.GameSystem;
using Systems.GameSystem.Config;
using Systems.GameSystem.Signals;
using Systems.InputSystem.Controller;
using Systems.MainMenuSystem.Model;
using Systems.MainMenuSystem.View;
using UniRx;
using UnityEngine;
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

        private CompositeDisposable _disposable;

        public MainMenuCanvasController(MainMenuCanvasModel model, MainMenuCanvasView view, InputMaster inputMaster, GameConfig gameConfig, VirtualKeyboardController virtualKeyboardController, SignalBus signalBus)
        {
            _model = model;
            _view = view;
            _inputMaster = inputMaster;
            _signalBus = signalBus;
            _disposable = new CompositeDisposable();
            
            PressButtonToStart();
        }
        
        private Action<InputAction.CallbackContext> _pressButtonToStartAction;

        private void PressButtonToStart()
        {
            _inputMaster.Enable();
            _inputMaster.UiControl.Enable();
            _pressButtonToStartAction = _=> OnButtonPressed();
            _inputMaster.UiControl.Submit.performed += _pressButtonToStartAction;
        }

        private void OnButtonPressed()
        {
            _view.gameObject.SetActive(false);
            _signalBus.Fire<NameInputSignal>();
            _inputMaster.UiControl.Submit.performed -= _pressButtonToStartAction;
        }
        
        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}