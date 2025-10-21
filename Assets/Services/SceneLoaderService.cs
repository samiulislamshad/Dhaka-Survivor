using System;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Services
{
    [Serializable]
    public class SceneLoaderService : IDisposable
    {
        private readonly CompositeDisposable _disposables = new();

        public async UniTask LoadSceneAsync(string sceneToLoad, string sceneToUnload = null,
            IReactiveProperty<float> progressTracker = null)
        {
            var operation = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);
            
            if (progressTracker != null)
            {
                operation.AsAsyncOperationObservable()
                    .Subscribe(op => progressTracker.Value = op.progress)
                    .AddTo(_disposables);
            }
            
            await operation;
            if(progressTracker != null)
                progressTracker.Value = 1f;
            await UniTask.Delay(2000);
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneToLoad));
            
            if (!string.IsNullOrEmpty(sceneToUnload))
                await UnloadSceneAsync(sceneToUnload);
        }

        public async UniTask UnloadSceneAsync(string sceneToUnload)
        {
            await SceneManager.UnloadSceneAsync(sceneToUnload);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}