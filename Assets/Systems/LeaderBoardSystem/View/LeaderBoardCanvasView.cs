using System.Collections.Generic;
using Systems.InputSystem.Model;
using Systems.LeaderBoardSystem.Signal;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace Systems.LeaderBoardSystem.View
{
    public class LeaderBoardCanvasView : MonoBehaviour
    {
        [Inject] public EventSystem eventSystem;
        public Button mainMenuButton;
        public Button retryButton;
        
        [Header("References")] public RectTransform scrollViewContent;
        public ScrollRect scrollRect;
        public GameObject elementPrefab;

        [Header("Settings")] public int poolSize = 20;
        public float spacing = 5f;
        public float elementHeight = 100f;

        [SerializeField] private List<UserDataView> elementPool = new();
        private readonly Queue<UserDataView> _availableElements = new();
        private readonly Dictionary<int, UserDataView> _activeElements = new();

        // Sticky element for current player when outside visible range
        private UserDataView _stickyElement;
        private bool _isShowingStickyElement;

        // Events
        public readonly Subject<Unit> OnViewInitialized = new();
        public readonly Subject<float> OnScrollValueChanged = new();

        private int _totalItems;
        private float _contentHeight;
        private int _currentPlayerRank = -1;

        private void Awake()
        {
            RemoveLayoutComponents();

            _isShowingStickyElement = false;
        }

        public void Initialize(int totalItems, int currentPlayerRank)
        {
            _totalItems = totalItems;
            _currentPlayerRank = currentPlayerRank;

            RemoveLayoutComponents();
            ConfigureScrollRect();
            CreateElementPool();
            CreateStickyElement();
            SetupScrollContent();

            Debug.Log($"LeaderBoardView initialized with {totalItems} items, current player rank: {currentPlayerRank}");

            OnViewInitialized.OnNext(Unit.Default);
        }

        private void Update()
        {
            if (!gameObject.activeInHierarchy) return;
        
            // Only set focus if nothing is selected or we're not scrolling
            if (eventSystem.currentSelectedGameObject == null && !isScrollFocused)
            {
                eventSystem.SetSelectedGameObject(mainMenuButton.gameObject);
            }
        }
        
        
        public bool isScrollFocused = false;

        [SerializeField]
        private float scrollSpeed;
        public void HandleScrollInput(ScrollNavigationSignal signal)
        {
            var input = signal.scrollInput;
            if (Mathf.Abs(input.y) > 0.1f)
            {
                isScrollFocused = true;
            
                // Scroll the content
                float scrollAmount = input.y * Time.deltaTime * scrollSpeed;
                scrollRect.verticalNormalizedPosition = Mathf.Clamp01(
                    scrollRect.verticalNormalizedPosition + scrollAmount
                );
            
                // Optional: Clear selection while scrolling
                eventSystem.SetSelectedGameObject(null);
            }
            else
            {
                isScrollFocused = false;
            }
        }

        private void RemoveLayoutComponents()
        {
            ContentSizeFitter sizeFitter = scrollViewContent.GetComponent<ContentSizeFitter>();
            if (sizeFitter != null) DestroyImmediate(sizeFitter);

            VerticalLayoutGroup layoutGroup = scrollViewContent.GetComponent<VerticalLayoutGroup>();
            if (layoutGroup != null) DestroyImmediate(layoutGroup);

            HorizontalLayoutGroup horizontalLayout = scrollViewContent.GetComponent<HorizontalLayoutGroup>();
            if (horizontalLayout != null) DestroyImmediate(horizontalLayout);
        }

        private void ConfigureScrollRect()
        {
            scrollRect.vertical = true;
            scrollRect.horizontal = false;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.inertia = true;
        }

        private void CreateElementPool()
        {
            for (int i = 0; i < poolSize; i++)
            {
                var elementObj = Instantiate(elementPrefab, scrollViewContent);
                var elementView = elementObj.GetComponent<UserDataView>();

                if (elementView == null)
                {
                    elementView = elementObj.AddComponent<UserDataView>();
                }

                SetupElementRectTransform(elementView.RectTransform);

                elementPool.Add(elementView);
                _availableElements.Enqueue(elementView);
                elementObj.SetActive(false);
            }
        }

        private void CreateStickyElement()
        {
            // Create a separate element for the sticky indicator
            var stickyObj = Instantiate(elementPrefab, scrollViewContent);
            _stickyElement = stickyObj.GetComponent<UserDataView>();

            if (_stickyElement == null)
            {
                _stickyElement = stickyObj.AddComponent<UserDataView>();
            }

            SetupElementRectTransform(_stickyElement.RectTransform);
            stickyObj.SetActive(false);
        }

        private void SetupElementRectTransform(RectTransform rect)
        {
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(0, elementHeight);
            rect.anchoredPosition = Vector2.zero;
        }

        private void SetupScrollContent()
        {
            scrollViewContent.anchorMin = new Vector2(0, 1);
            scrollViewContent.anchorMax = new Vector2(1, 1);
            scrollViewContent.pivot = new Vector2(0.5f, 1f);

            CalculateAndSetContentSize();
            scrollViewContent.anchoredPosition = Vector2.zero;

            scrollRect.onValueChanged.AsObservable()
                .Subscribe(value => OnScrollValueChanged.OnNext(value.y))
                .AddTo(this);
        }

        public void UpdateVisibleElements(int startIndex, int endIndex, List<UserData> userData,
            float scrollPosition = 0f)
        {
            if (userData == null) return;

            // Update sticky element visibility based on current player position
            UpdateStickyElement(startIndex, endIndex, userData);

            // Return elements that are no longer visible
            var keysToRemove = new List<int>();
            foreach (var kvp in _activeElements)
            {
                if (kvp.Key < startIndex || kvp.Key > endIndex)
                {
                    ReturnElementToPool(kvp.Value);
                    keysToRemove.Add(kvp.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                _activeElements.Remove(key);
            }

            // Get elements for visible indices
            for (int i = startIndex; i <= endIndex; i++)
            {
                if (i >= 0 && i < userData.Count && !_activeElements.ContainsKey(i))
                {
                    var element = GetElementFromPool();
                    if (element != null)
                    {
                        SetupElementPosition(element, i, userData[i]);
                        _activeElements[i] = element;
                    }
                }
            }
        }

        private void UpdateStickyElement(int startIndex, int endIndex, List<UserData> userData)
        {
            if (_currentPlayerRank <= 0 || _currentPlayerRank > userData.Count) return;

            var currentPlayerData = userData.Find(user => user.isCurrentPlayer);
            if (currentPlayerData == null) return;

            bool isCurrentPlayerVisible = (_currentPlayerRank >= startIndex && _currentPlayerRank <= endIndex);

            if (isCurrentPlayerVisible)
            {
                // Current player is in visible range, hide sticky element
                if (_isShowingStickyElement)
                {
                    _stickyElement.gameObject.SetActive(false);
                    _isShowingStickyElement = false;
                }
            }
            else
            {
                // Current player is outside visible range, show sticky element
                if (!_isShowingStickyElement)
                {
                    _stickyElement.gameObject.SetActive(true);
                    _isShowingStickyElement = true;
                }

                // Position sticky element and update its data
                PositionStickyElement(startIndex, endIndex, currentPlayerData);
            }
        }

        private void PositionStickyElement(int startIndex, int endIndex, UserData currentPlayerData)
        {
            var stickyYPosition = 0f;
            var positionText = "";

            if (_currentPlayerRank < startIndex)
            {
                // Current player is above the visible range - show at top
                stickyYPosition = 0f;
                positionText = $"↑ Rank {_currentPlayerRank} is above";
            }
            else if (_currentPlayerRank > endIndex)
            {
                // Current player is below the visible range - show at bottom
                float viewportHeight = scrollRect.viewport.rect.height;
                stickyYPosition = -viewportHeight + elementHeight;
                positionText = $"↓ Rank {_currentPlayerRank} is below";
            }

            // Position the sticky element within the viewport (not in content)
            RectTransform stickyRect = _stickyElement.RectTransform;
            stickyRect.SetParent(scrollRect.viewport);
            stickyRect.anchoredPosition = new Vector2(0, stickyYPosition);
            stickyRect.SetParent(scrollViewContent); // Return to content for proper ordering

            // Create a modified data for the sticky indicator
            var stickyData = new UserData(
                currentPlayerData.rank,
                currentPlayerData.userName,
                currentPlayerData.score,
                currentPlayerData.userId,
                true
            );

            _stickyElement.UpdateElement(stickyData, true);
        }

        private UserDataView GetElementFromPool()
        {
            if (_availableElements.Count > 0)
            {
                return _availableElements.Dequeue();
            }

            return null;
        }

        private void ReturnElementToPool(UserDataView element)
        {
            element.gameObject.SetActive(false);
            _availableElements.Enqueue(element);
        }

        private void SetupElementPosition(UserDataView element, int index, UserData userData)
        {
            float yPos = -index * (elementHeight + spacing);
            element.RectTransform.anchoredPosition = new Vector2(0, yPos);
            element.gameObject.SetActive(true);
            element.UpdateElement(userData);
        }

        public void RefreshContentSize(int newTotalItems)
        {
            _totalItems = newTotalItems;
            CalculateAndSetContentSize();
        }

        private void CalculateAndSetContentSize()
        {
            _contentHeight = _totalItems * (elementHeight + spacing);
            scrollViewContent.sizeDelta = new Vector2(0, _contentHeight);
        }

        public void ClearAllElements()
        {
            foreach (var kvp in _activeElements)
            {
                ReturnElementToPool(kvp.Value);
            }

            _activeElements.Clear();

            if (_stickyElement != null)
            {
                _stickyElement.gameObject.SetActive(false);
                _isShowingStickyElement = false;
            }
        }

        public float GetElementHeightWithSpacing()
        {
            return elementHeight + spacing;
        }
    }
}