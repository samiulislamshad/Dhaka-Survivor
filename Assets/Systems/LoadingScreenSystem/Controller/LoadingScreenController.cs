using System;
using Systems.LoadingScreenSystem.Model;
using Systems.LoadingScreenSystem.View;
using UniRx;
using UnityEngine;

namespace Systems.LoadingScreenSystem.Controller
{
    [Serializable]
    public class LoadingScreenController : IDisposable
    {
        private LoadingScreenCanvasView _view;
        private CompositeDisposable _disposable;

        public ReactiveProperty<float> progress;

        public LoadingScreenController(LoadingScreenModel model, LoadingScreenCanvasView view)
        {
            _view = view;

            _disposable = new CompositeDisposable();
            progress = new ReactiveProperty<float>();
            
            SubscribeToProperties();
        }

        private void SubscribeToProperties()
        {
            progress.Subscribe(value => _view.SetSliderValue(value)).AddTo(_disposable);
        }

        public void Initialize()
        {
            progress.Value = 0;
            ShowLoadingScreen();
        }

        public void ShowLoadingScreen() => _view.loadingScreen.SetActive(true);
        public void HideLoadingScreen() => _view.loadingScreen.SetActive(false);
        
        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}