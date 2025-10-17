using System;
using Cysharp.Threading.Tasks;
using Systems.MainMenuSystem.Model;
using Systems.MainMenuSystem.View;
using UniRx;

namespace Systems.MainMenuSystem.Controller
{
    [Serializable]
    public class MainMenuCanvasController : IDisposable
    {
        private readonly MainMenuCanvasModel _model;
        private readonly MainMenuCanvasView _view;
        
        private CompositeDisposable _disposable;

        public MainMenuCanvasController(MainMenuCanvasModel model, MainMenuCanvasView view)
        {
            _model = model;
            _view = view;
            _disposable = new CompositeDisposable();
            
            SubscribeToProperties();
        }

        private void SubscribeToProperties()
        {
            _view.newGameButton.OnClickAsObservable().Subscribe(_ =>
            {
                StartNewGame().Forget();
            }).AddTo(_disposable);
        }

        public async UniTask StartNewGame()
        {
            await _model.StartNewGame();
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}