using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Systems.GameSystem.Config;
using Systems.InputSystem.Model;
using Systems.LeaderBoardSystem.Model;
using Systems.LeaderBoardSystem.Signal;
using Systems.LeaderBoardSystem.View;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;

namespace Systems.LeaderBoardSystem.Controller
{
    [Serializable]
    public class LeaderBoardController : MonoBehaviour
    {
        [Header("Dependencies")] 
        private InputMaster _inputMaster;
        private SignalBus _signalBus;
        private LeaderBoardCanvasView _view;
        private LeaderBoardModel _model;
        private GameConfig _gameConfig;
        private CompositeDisposable _disposables;
        
        private ScrollNavigationSignal _scrollNavigationSignal;

        private int _visibleItemCount;
        private int _currentStartIndex = 0;
        private int _currentEndIndex = 0;
        private float _currentScrollPosition = 0f;
        

        [Inject]
        private void InjectReference(LeaderBoardModel model,
            LeaderBoardCanvasView view,
            InputMaster inputMaster,
            SignalBus signalBus,
            GameConfig gameConfig)
        {
            _model = model;
            _view = view;
            _inputMaster = inputMaster;
            _signalBus = signalBus;
            _gameConfig = gameConfig;

            _disposables = new CompositeDisposable();
            
            SubscribeToSignals();
            SubscribeToProperties();
        }

        private void SubscribeToSignals()
        {
            _inputMaster.Enable();
            _inputMaster.UiControl.Enable();
            _inputMaster.UiControl.ScrollWheel.Enable();
            _inputMaster.UiControl.ScrollWheel.performed += ScrollWheelInput;
            
            _signalBus.Subscribe<ScrollNavigationSignal>(_view.HandleScrollInput);
        }
        
        private void UnsubscribeToSignals()
        {
            _inputMaster.UiControl.Disable();
            _inputMaster.UiControl.ScrollWheel.Disable();
            _inputMaster.UiControl.ScrollWheel.performed -= ScrollWheelInput;
            
            _signalBus.Unsubscribe<ScrollNavigationSignal>(_view.HandleScrollInput);
        }

        private void ScrollWheelInput(InputAction.CallbackContext callbackContext)
        {
            _scrollNavigationSignal ??= new ScrollNavigationSignal(Vector2.zero);
            _scrollNavigationSignal.scrollInput = callbackContext.ReadValue<Vector2>();
            _signalBus.Fire(_scrollNavigationSignal);
        }

        private void SubscribeToProperties()
        {
            _view.mainMenuButton.OnClickAsObservable().Subscribe(_ => { OnBackToMainMenu(); }).AddTo(_disposables);
            _view.retryButton.OnClickAsObservable().Subscribe(_ => { OnRetry(); }).AddTo(_disposables);
            
            _view.OnViewInitialized
                .Subscribe(_ => SetupRecyclableScrollView())
                .AddTo(_disposables);
        }

        private void Start()
        {
            Initialize().Forget();
        }

        private async UniTaskVoid Initialize()
        {
            await _model.InitializeLeaderBoardData();
            _view.Initialize(_model.totalUserCount.Value, _model.currentPlayerRank.Value);
        }
        
        private async UniTaskVoid OnBackToMainMenu()
        {
            _view.mainMenuButton.interactable = false;
            await _model.AddPlayerDataToLeaderboard();
            SceneManager.LoadScene("Game");
        }

        private void OnRetry()
        {
            _view.retryButton.interactable = false;
            _gameConfig.isRetrying.Value = true;
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
            UnsubscribeToSignals();
            _disposables.Dispose();
        }
    }
}