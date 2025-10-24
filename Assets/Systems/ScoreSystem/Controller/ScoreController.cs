using System;
using Cysharp.Threading.Tasks;
using Services;
using Systems.GameSystem.Config;
using Systems.PauseSystem.Signals;
using Systems.PlayerSystem.Signals;
using Systems.ScoreSystem.Signal;
using UniRx;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Zenject;

namespace Systems.ScoreSystem.Controller
{
    [Serializable]
    public class ScoreController : IDisposable
    {
        private readonly ScoreCanvasView _view;
        private readonly GameConfig _gameConfig;
        private readonly SignalBus _signalBus;
        private readonly SceneLoaderService _sceneLoaderService;
        
        private readonly CompositeDisposable _disposable;

        private ReactiveProperty<int> _score;
        
        public ScoreController(ScoreCanvasView view, GameConfig gameConfig, SignalBus signalBus, SceneLoaderService sceneLoaderService)
        {
            _view = view;
            _gameConfig = gameConfig;
            _signalBus = signalBus;
            _sceneLoaderService = sceneLoaderService;
            
            _disposable = new CompositeDisposable();
            _score = new ReactiveProperty<int>(0);

            SubscribeToProperties();
            SubscribeToSignals();
        }

        private void SubscribeToProperties()
        {
            _gameConfig.hasGameStarted.Subscribe(value =>
            {
                _view.runStartScorePanel.SetActive(value);
            }).AddTo(_disposable);

            _score.Subscribe(value =>
            {
                _view.playerScore.text = value.ToString();
            }).AddTo(_disposable);

            _view.okayButton.OnClickAsObservable().Subscribe(_ =>
            {
                HideScoreBoard();
            }).AddTo(_disposable);
        }

        private void SubscribeToSignals()
        {
            _signalBus.Subscribe<AddScoreSignal>(AddScore);
            _signalBus.Subscribe<PlayerDeadSignal>(ShowScoreBoard);
        }
        
        private void UnsubscribeToSignals()
        {
            _signalBus.Unsubscribe<AddScoreSignal>(AddScore);
            _signalBus.Unsubscribe<PlayerDeadSignal>(ShowScoreBoard);
        }

        private void AddScore(AddScoreSignal signal)
        {
            _score.Value += signal.score;
        }

        private void ShowScoreBoard()
        {
            _gameConfig.currentUserData.score = _score.Value.ToString();
            
            _view.userName.text = _gameConfig.currentUserData.userName;
            _view.score.text = _score.Value.ToString();
            _view.okayButton.interactable = true;
            _view.runEndScorePanel.SetActive(true);
            EventSystem.current.SetSelectedGameObject(_view.okayButton.gameObject);
        }

        private void HideScoreBoard()
        {
            _view.okayButton.interactable = false;
            SceneManager.LoadScene("Leaderboard");
            _view.runEndScorePanel.SetActive(false);
            _signalBus.Fire<UnpauseSignal>();
        }

        public void Dispose()
        {
            UnsubscribeToSignals();
            _disposable.Dispose();
        }
    }
}