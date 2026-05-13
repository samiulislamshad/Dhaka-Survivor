using System;
using Systems.AudioSystem.Manager;
using Systems.EnemySystem.Controller;
using Systems.GameSystem.Config;
using Systems.GameSystem.Signals;
using Systems.GameSystem.View;
using Systems.InputSystem.Service;
using Systems.PlayerSystem.Signals.GameSignals;
using Systems.ScoreSystem.Signal;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace Systems.GameSystem.Manager
{
    public class GameManager : MonoBehaviour, IDisposable
    {
        private GameConfig _config;
        private EnemyController _enemyController;
        private SignalBus _signalBus;
        private CompositeDisposable _disposable;
        private InputMaster _inputMaster;
        private AudioManager _audioManager;
        private InputDeviceDetector _inputDeviceDetector;
        
        [SerializeField] private StartGameCanvasView startGameCanvasView;

        [Inject]
        public void InitializeDiReference(
            EnemyController enemyController,
            StartGameCanvasView view,
            SignalBus signalBus,
            GameConfig config,
            InputMaster inputMaster,
            AudioManager audioManager,
            InputDeviceDetector inputDeviceDetector)
        {
            _enemyController = enemyController;
            _signalBus = signalBus;
            startGameCanvasView = view;
            _config = config;
            _inputMaster = inputMaster;
            _audioManager = audioManager;
            _inputDeviceDetector = inputDeviceDetector;

            _disposable = new CompositeDisposable();
            
            InitializeGame();
        }

        private void InitializeGame()
        {
            InitializeVariables();
            SubscribeToSignals();
            SubscribeToProperties();
        }

        private void InitializeVariables()
        {
            _config.gameSpeed = new ReactiveProperty<float>(10);
            _config.timer = new ReactiveProperty<float>(0);
            _config.hasGameStarted = new ReactiveProperty<bool>(false);
            _config.hasTimerStarted = new ReactiveProperty<bool>(false);
            _config.maxEnemies = new ReactiveProperty<int>(10);
            _config.gamePhase = new ReactiveProperty<GamePhase>(GamePhase.MainMenuScreen);

            _disposable = new CompositeDisposable();
            Time.timeScale = 1;
        }

        private void SubscribeToProperties()
        {
            _inputDeviceDetector.CurrentDevice.Subscribe(value =>
            {
                startGameCanvasView.ToggleKeyMapBasedOnInput(value);
            }).AddTo(_disposable);
        }

        private void SubscribeToSignals()
        {
            _signalBus.Subscribe<GameScreenSignal>(ShowKeyMappingUi);
            _signalBus.Subscribe<PlayerDeadSignal>(OnPlayerDeath);
        }

        private void UnsubscribeFromSignals()
        {
            _signalBus.Unsubscribe<GameScreenSignal>(ShowKeyMappingUi);
            _signalBus.Unsubscribe<PlayerDeadSignal>(OnPlayerDeath);
        }

        private Action<InputAction.CallbackContext> _startGameInputAction;
        private void ShowKeyMappingUi()
        {
            startGameCanvasView.ToggleKeymappingPanel(true);
            startGameCanvasView.ToggleKeyMapBasedOnInput(_inputDeviceDetector.CurrentDevice.Value);

            startGameCanvasView.keyboardStartButton.OnClickAsObservable()
                .Subscribe(_ =>
                {
                    HideKeyMappingUi();
                }).AddTo(_disposable);

            startGameCanvasView.touchScreenStartButton.OnClickAsObservable()
                .Subscribe(_ =>
                {
                    HideKeyMappingUi();
                }).AddTo(_disposable);
            
            _startGameInputAction = _ => HideKeyMappingUi();
            _inputMaster.UiControl.Submit.performed += _startGameInputAction;
        }

        private void HideKeyMappingUi()
        {
            startGameCanvasView.ToggleKeymappingPanel(false);
            _inputMaster.UiControl.Submit.performed -= _startGameInputAction;
            _inputMaster.Enable();
            _inputMaster.PlayerControl.Enable();
            _config.gamePhase.Value = GamePhase.GameScreen;
            StartGame();
        }

        private void StartGame()
        {
            _config.hasGameStarted.Value = true;
            _config.hasTimerStarted.Value = true;
        }
        
        private AddScoreSignal _addScoreSignal;
        private void FixedUpdate()
        {
            if (_config.hasTimerStarted.Value)
                _config.timer.Value += Time.fixedDeltaTime;
           
            if(_config.hasGameStarted.Value)
                _config.gameSpeed.Value += Time.fixedDeltaTime/5 * 1.5f;
        }

        private void OnPlayerDeath()
        {
            _audioManager.StopMainMenuMusic();
        }
        
        private void OnDestroy()
        {
            Dispose();
        }

        public void Dispose()
        {
            _inputMaster.PlayerControl.Disable();
            
            UnsubscribeFromSignals();
            _disposable?.Dispose();
            _enemyController?.Dispose();
        }
    }
}