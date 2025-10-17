using System;
using Cysharp.Threading.Tasks;
using Services;
using Systems.LoadingScreenSystem.Controller;
using UniRx;
using UnityEngine.SceneManagement;

namespace Systems.MainMenuSystem.Model
{
    [Serializable]
    public class MainMenuCanvasModel : IDisposable
    {
        private readonly SceneLoaderService _sceneLoaderService;
        private LoadingScreenController _loadingScreenController;
        private CompositeDisposable _disposable;

        public MainMenuCanvasModel (LoadingScreenController loadingScreenController, 
            SceneLoaderService sceneLoaderService)
        {
            _loadingScreenController = loadingScreenController;
            _sceneLoaderService = sceneLoaderService;

            _disposable = new CompositeDisposable();
        }

        public async UniTask StartNewGame()
        {
            _loadingScreenController.Initialize();
            var currentScene = SceneManager.GetActiveScene();
            await UniTask.Delay(2000);
            await _sceneLoaderService.LoadSceneAsync("Game", currentScene.name);
        }

        private async UniTask ProgressBar()
        {
            
        }

        public void ShowLeaderboard()
        {
            
        }

        public void ShowOptions()
        {
            
        }

        public void ShowCredits()
        {
            
        }

        public void Quit()
        {
            
        }
        
        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}