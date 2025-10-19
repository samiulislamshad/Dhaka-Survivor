using System;
using Systems.EnemySystem.Controller;
using Systems.GameSystem.Config;
using Systems.GameSystem.View;
using Systems.PlayerSystem.Signals;
using UniRx;
using UnityEngine;
using Zenject;

namespace Systems.GameSystem.Manager
{
    public class GameManager : MonoBehaviour, IDisposable
    {
        private GameConfig _config;
        private EnemyController _enemyController;
        private SignalBus _signalBus;
        private CompositeDisposable _disposable;
        
        [SerializeField] private StartGameCanvasView startGameCanvasView;

        [Inject]
        public void InitializeDiReference(
            EnemyController enemyController,
            StartGameCanvasView view,
            SignalBus signalBus,
            GameConfig config)
        {
            _enemyController = enemyController;
            _signalBus = signalBus;
            startGameCanvasView = view;
            _config = config;
        }

        private void Awake()
        {
            InitializeGame();
        }

        public void InitializeGame()
        {
            InitializeVariables();
            SubscribeToProperties();
            ShowPressToStartGame();
        }

        private void InitializeVariables()
        {
            _config.gameSpeed = new ReactiveProperty<int>(5);
            _config.timer = new ReactiveProperty<float>(0);
            _config.hasGameStarted = new ReactiveProperty<bool>(false);
            _config.hasTimerStarted = new ReactiveProperty<bool>(false);

            // NEW: Initialize spawn settings
            _config.maxEnemies = new ReactiveProperty<int>(10);

            _disposable = new CompositeDisposable();
        }

        private void SubscribeToProperties()
        {
            // Increment game speed every 10 seconds
            _config.timer
                .Where(_ => _config.hasGameStarted.Value)
                .Where(value => Mathf.FloorToInt(value) % 10 == 0 && Mathf.FloorToInt(value) > 0)
                .DistinctUntilChanged(value => Mathf.FloorToInt(value / 10)) // Prevent multiple calls per interval
                .Subscribe(_ => IncrementGameSpeed())
                .AddTo(_disposable);
        }

        private void ShowPressToStartGame()
        {
            startGameCanvasView.startGamePanel.SetActive(true);
            _signalBus.Subscribe<StartJumpInputSignal>(HidePressToStartGame);
        }

        private void HidePressToStartGame()
        {
            _signalBus.Unsubscribe<StartJumpInputSignal>(HidePressToStartGame);
            startGameCanvasView.startGamePanel.SetActive(false);
            StartGame();
        }

        private void StartGame()
        {
            _config.hasGameStarted.Value = true;
            _config.hasTimerStarted.Value = true;
        }

        private void FixedUpdate()
        {
            if (_config.hasTimerStarted.Value)
                _config.timer.Value += Time.fixedDeltaTime;
        }

        private void IncrementGameSpeed()
        {
            if (!_config.hasGameStarted.Value) return;
            if (!_config.hasTimerStarted.Value) return;
            _config.gameSpeed.Value++;
        }

        private void OnDestroy()
        {
            Dispose();
        }

        public void Dispose()
        {
            _disposable?.Dispose();
            _enemyController?.Dispose();
        }
    }
}