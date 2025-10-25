using System;
using Systems.LeaderBoardSystem.Model;
using Systems.LeaderBoardSystem.View;
using UniRx;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Systems.LeaderBoardSystem.Controller
{
    [Serializable]
    public class LeaderBoardController : IDisposable
    {
        private LeaderBoardCanvasView _canvasView;
        private LeaderBoardModel _model;
        private CompositeDisposable _disposable;
        private EventSystem _eventSystem;

        public LeaderBoardController(LeaderBoardModel model, LeaderBoardCanvasView canvasView)
        {
            _model = model;
            _canvasView = canvasView;
            
            _canvasView.eventSystem.SetSelectedGameObject(_canvasView.mainMenuButton.gameObject);
            _disposable = new CompositeDisposable();

            SubscribeToProperties();
        }

        private void SubscribeToProperties()
        {
            _canvasView.mainMenuButton
                .OnClickAsObservable()
                .Subscribe(_ => { SceneManager.LoadScene("Game"); }).AddTo(_disposable);

            Observable.EveryUpdate()
                .Where(_ =>
                    _canvasView.eventSystem.currentSelectedGameObject != _canvasView.mainMenuButton.gameObject)
                .Subscribe(_ => { _eventSystem.SetSelectedGameObject(_canvasView.mainMenuButton.gameObject); })
                .AddTo(_disposable);
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}