using System;
using System.Globalization;
using System.Linq;
using Cysharp.Threading.Tasks;
using Services;
using Systems.GameSystem.Config;
using Systems.GameSystem.Signals;
using Systems.LeaderBoardSystem.Manager;
using Systems.LeaderBoardSystem.Scriptable;
using Systems.PauseSystem.Signals;
using Systems.PlayerSystem.Signals.GameSignals;
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
        
        private readonly PlayFabLeaderboardManager _leaderboardManager;
        private LeaderBoardScriptable _leaderBoardScriptable;

        private readonly CompositeDisposable _disposable;

        private ReactiveProperty<int> _score;
        private float _lastTimerValue = 0;
        public ScoreController(ScoreCanvasView view, 
            GameConfig gameConfig, 
            SignalBus signalBus,
            SceneLoaderService sceneLoaderService, 
            PlayFabLeaderboardManager leaderboardManager, 
            LeaderBoardScriptable leaderBoardScriptable)
        {
            _view = view;
            _gameConfig = gameConfig;
            _signalBus = signalBus;
            _leaderboardManager = leaderboardManager;
            _leaderBoardScriptable = leaderBoardScriptable;

            _disposable = new CompositeDisposable();
            _score = new ReactiveProperty<int>(0);

            SubscribeToProperties();
            SubscribeToSignals();
        }

        private void SubscribeToProperties()
        {
            Observable.EveryFixedUpdate()
                .Subscribe(_ =>
                {
                    var currentTimer = _gameConfig.timer.Value;
                    if (!(currentTimer - _lastTimerValue >= 1)) return;
                    _score.Value += 100;
                    _lastTimerValue = currentTimer;
                })
                .AddTo(_disposable);

            _score.Subscribe(value => { _view.playerScore.text = value.ToString(); }).AddTo(_disposable);

            _view.okayButton.OnClickAsObservable().Subscribe(_ => { HideScoreBoard(); }).AddTo(_disposable);
        }

        private void SubscribeToSignals()
        {
            _signalBus.Subscribe<AddScoreSignal>(AddScore);
            _signalBus.Subscribe<PlayerDeadSignal>(ShowScoreBoard);
            _signalBus.Subscribe<GameScreenSignal>(ShowPlayerScore);
        }

        private void UnsubscribeToSignals()
        {
            _signalBus.Unsubscribe<AddScoreSignal>(AddScore);
            _signalBus.Unsubscribe<PlayerDeadSignal>(ShowScoreBoard);
            _signalBus.Unsubscribe<GameScreenSignal>(ShowPlayerScore);
        }

        private void AddScore(AddScoreSignal signal)
        {
            _score.Value += signal.score;
        }

        private float _scoreToView;

        private void ShowScoreBoard()
        {
            _gameConfig.currentUserData.score = _score.Value;
            _view.runStartScorePanel.SetActive(false);
            _view.runEndScorePanel.SetActive(true);
            _scoreToView = _score.Value;
            _view.playerScore.text = _scoreToView.ToString(CultureInfo.InvariantCulture);
            
            _view.animator.Play($"SadAnimation");
            ShowScore().Forget();
        }

        private async UniTaskVoid ShowScore()
        {
            await UniTask.Delay(2000,DelayType.UnscaledDeltaTime);
            _view.scorePanel.SetActive(true);
            _view.userName.text = _gameConfig.currentUserData.userName;
            _view.score.text = _scoreToView.ToString(CultureInfo.InvariantCulture); //_score.Value.ToString();
            await UniTask.Delay(2000,DelayType.UnscaledDeltaTime);
            _view.okayButton.gameObject.SetActive(true);
            _view.okayButton.interactable = true;
            EventSystem.current.SetSelectedGameObject(_view.okayButton.gameObject);
        }

        private void HideScoreBoard()
        {
            _view.okayButton.interactable = false;
            SceneManager.LoadScene("Leaderboard");
            _view.runEndScorePanel.SetActive(false);
            _signalBus.Fire<UnpauseSignal>();
        }

        private void ShowPlayerScore()
        {
            _view.runStartScorePanel.SetActive(true);
            ShowHighScore().Forget();
        }

        private async UniTaskVoid ShowHighScore()
        {
            var leaderboard = await _leaderboardManager.FetchLeaderboard(100);
            var sorted = leaderboard.OrderByDescending(user => user.score).ToList();
            var highScore = sorted[0].score;
            _view.highScore.text = highScore.ToString();
        }

        public void Dispose()
        {
            UnsubscribeToSignals();
            _disposable.Dispose();
        }
    }
}