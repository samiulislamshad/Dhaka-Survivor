using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Systems.InputSystem.Model;
using Systems.LeaderBoardSystem.Model;
using Systems.LeaderBoardSystem.View;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;

namespace Systems.LeaderBoardSystem.Controller
{
    [Serializable]
    public class LeaderBoardController : MonoBehaviour
    {
        [Header("Dependencies")] 
        private LeaderBoardCanvasView _view;
        private LeaderBoardModel _model;
        private CompositeDisposable _disposables;

        private int _visibleItemCount;
        private int _currentStartIndex = 0;
        private int _currentEndIndex = 0;
        private float _currentScrollPosition = 0f;

        [Inject]
        private void InjectReference(LeaderBoardModel model, LeaderBoardCanvasView view)
        {
            _model = model;
            _view = view;

            _disposables = new CompositeDisposable();
            
            SubscribeToProperties();
        }

        private void SubscribeToProperties()
        {
            _view.mainMenuButton.OnClickAsObservable().Subscribe(_ => { OnBackToMainMenu(); }).AddTo(_disposables);
            
            _view.OnViewInitialized
                .Subscribe(_ => SetupRecyclableScrollView())
                .AddTo(_disposables);
        }

        private void Start()
        {
            Initialize();
        }

        private async UniTaskVoid Initialize()
        {
            await _model.InitializeLeaderBoardData();
            _view.Initialize(_model.totalUserCount.Value, _model.currentPlayerRank.Value);
        }
        
        private void OnBackToMainMenu()
        {
            _view.mainMenuButton.interactable = false;
            SceneManager.LoadScene("Game");
        }

        #region Recyclable Scroll View

        private void SetupRecyclableScrollView()
        {
            StartCoroutine(SetupAfterLayout());
        }

        private IEnumerator SetupAfterLayout()
        {
            yield return new WaitForEndOfFrame();

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(_view.scrollRect.viewport as RectTransform);

            float viewportHeight = _view.scrollRect.viewport.rect.height;
            float elementHeightWithSpacing = _view.GetElementHeightWithSpacing();

            if (viewportHeight <= 0)
            {
                viewportHeight = 600f;
                Debug.LogWarning($"Viewport height was 0, using fallback: {viewportHeight}");
            }

            _visibleItemCount = Mathf.CeilToInt(viewportHeight / elementHeightWithSpacing) + 2;

            _view.OnScrollValueChanged
                .Subscribe(OnScrollValueChanged)
                .AddTo(_disposables);

            _model.userDataList
                .Subscribe(OnUserDataChanged)
                .AddTo(_disposables);

            UpdateVisibleElements(0);
        }

        private void OnScrollValueChanged(float scrollValue)
        {
            _currentScrollPosition = scrollValue;

            if (_model.userDataList.Value == null || _model.userDataList.Value.Count == 0) return;

            var contentHeight = _view.scrollViewContent.rect.height;
            var viewportHeight = _view.scrollRect.viewport.rect.height;

            if (viewportHeight <= 0)
            {
                viewportHeight = (_view.scrollRect.viewport as RectTransform).rect.height;
            }

            if (contentHeight <= viewportHeight)
            {
                UpdateVisibleElements(0);
                return;
            }

            var scrollPos = (1f - scrollValue) * (contentHeight - viewportHeight);
            var newStartIndex = Mathf.FloorToInt(scrollPos / _view.GetElementHeightWithSpacing());
            newStartIndex = Mathf.Max(0, newStartIndex - 1);

            UpdateVisibleElements(newStartIndex);
        }

        private void UpdateVisibleElements(int startIndex)
        {
            if (_model.userDataList.Value == null) return;

            startIndex = Mathf.Clamp(startIndex, 0, _model.userDataList.Value.Count - 1);
            var endIndex = Mathf.Clamp(startIndex + _visibleItemCount - 1, 0, _model.userDataList.Value.Count - 1);

            _currentStartIndex = startIndex;
            _currentEndIndex = endIndex;

            _view.UpdateVisibleElements(startIndex, endIndex, _model.userDataList.Value, _currentScrollPosition);
        }

        private void OnUserDataChanged(List<UserData> userData)
        {
            if (userData == null) return;

            _view.RefreshContentSize(userData.Count);
            UpdateVisibleElements(_currentStartIndex);
        }

        #endregion

        public void OnDestroy()
        {
            _disposables.Dispose();
        }
    }
}