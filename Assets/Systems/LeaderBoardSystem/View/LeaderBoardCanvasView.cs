using System.Collections.Generic;
using System.Linq;
using Systems.InputSystem.Model;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.LeaderBoardSystem.View
{
    public class LeaderBoardCanvasView : MonoBehaviour
    {
        [Header("References")] public RectTransform scrollViewContent;
        public ScrollRect scrollRect;
        public GameObject elementPrefab;

        [Header("Settings")] public int poolSize = 20;
        public float spacing = 5f;
        public float elementHeight = 100f;

        private List<UserDataView> elementPool = new List<UserDataView>();
        private Queue<UserDataView> availableElements = new Queue<UserDataView>();
        private Dictionary<int, UserDataView> activeElements = new Dictionary<int, UserDataView>();

        // Sticky element for current player when outside visible range
        private UserDataView stickyElement;
        private bool isShowingStickyElement = false;

        // Events
        public Subject<Unit> onViewInitialized = new Subject<Unit>();
        public Subject<float> onScrollValueChanged = new Subject<float>();

        private int totalItems;
        private float contentHeight;
        private int currentPlayerRank = -1;

        void Awake()
        {
            RemoveLayoutComponents();
        }

        public void Initialize(int totalItems, int currentPlayerRank)
        {
            this.totalItems = totalItems;
            this.currentPlayerRank = currentPlayerRank;

            RemoveLayoutComponents();
            ConfigureScrollRect();
            CreateElementPool();
            CreateStickyElement();
            SetupScrollContent();

            Debug.Log($"LeaderBoardView initialized with {totalItems} items, current player rank: {currentPlayerRank}");

            onViewInitialized.OnNext(Unit.Default);
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
                availableElements.Enqueue(elementView);
                elementObj.SetActive(false);
            }
        }

        private void CreateStickyElement()
        {
            // Create a separate element for the sticky indicator
            var stickyObj = Instantiate(elementPrefab, scrollViewContent);
            stickyElement = stickyObj.GetComponent<UserDataView>();

            if (stickyElement == null)
            {
                stickyElement = stickyObj.AddComponent<UserDataView>();
            }

            SetupElementRectTransform(stickyElement.RectTransform);
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
                .Subscribe(value => onScrollValueChanged.OnNext(value.y))
                .AddTo(this);
        }

        public void UpdateVisibleElements(int startIndex, int endIndex, List<UserData> userData,
            float scrollPosition = 0f)
        {
            if (userData == null) return;

            // Update sticky element visibility based on current player position
            UpdateStickyElement(startIndex, endIndex, userData, scrollPosition);

            // Return elements that are no longer visible
            var keysToRemove = new List<int>();
            foreach (var kvp in activeElements)
            {
                if (kvp.Key < startIndex || kvp.Key > endIndex)
                {
                    ReturnElementToPool(kvp.Value);
                    keysToRemove.Add(kvp.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                activeElements.Remove(key);
            }

            // Get elements for visible indices
            for (int i = startIndex; i <= endIndex; i++)
            {
                if (i >= 0 && i < userData.Count && !activeElements.ContainsKey(i))
                {
                    var element = GetElementFromPool();
                    if (element != null)
                    {
                        SetupElementPosition(element, i, userData[i]);
                        activeElements[i] = element;
                    }
                }
            }
        }

        private void UpdateStickyElement(int startIndex, int endIndex, List<UserData> userData, float scrollPosition)
        {
            if (currentPlayerRank <= 0 || currentPlayerRank > userData.Count) return;

            var currentPlayerData = userData.Find(user => user.isCurrentPlayer);
            if (currentPlayerData == null) return;

            bool isCurrentPlayerVisible = (currentPlayerRank >= startIndex && currentPlayerRank <= endIndex);

            if (isCurrentPlayerVisible)
            {
                // Current player is in visible range, hide sticky element
                if (isShowingStickyElement)
                {
                    stickyElement.gameObject.SetActive(false);
                    isShowingStickyElement = false;
                }
            }
            else
            {
                // Current player is outside visible range, show sticky element
                if (!isShowingStickyElement)
                {
                    stickyElement.gameObject.SetActive(true);
                    isShowingStickyElement = true;
                }

                // Position sticky element and update its data
                PositionStickyElement(startIndex, endIndex, currentPlayerData, scrollPosition);
            }
        }

        private void PositionStickyElement(int startIndex, int endIndex, UserData currentPlayerData,
            float scrollPosition)
        {
            float stickyYPosition = 0f;
            string positionText = "";

            if (currentPlayerRank < startIndex)
            {
                // Current player is above the visible range - show at top
                stickyYPosition = 0f;
                positionText = $"↑ Rank {currentPlayerRank} is above";
            }
            else if (currentPlayerRank > endIndex)
            {
                // Current player is below the visible range - show at bottom
                float viewportHeight = scrollRect.viewport.rect.height;
                stickyYPosition = -viewportHeight + elementHeight;
                positionText = $"↓ Rank {currentPlayerRank} is below";
            }

            // Position the sticky element within the viewport (not in content)
            RectTransform stickyRect = stickyElement.RectTransform;
            stickyRect.SetParent(scrollRect.viewport);
            stickyRect.anchoredPosition = new Vector2(0, stickyYPosition);
            stickyRect.SetParent(scrollViewContent); // Return to content for proper ordering

            // Create a modified data for the sticky indicator
            var stickyData = new UserData(
                currentPlayerData.rank,
                positionText,
                currentPlayerData.score,
                currentPlayerData.userId,
                true
            );

            stickyElement.UpdateElement(stickyData, true);
        }

        private UserDataView GetElementFromPool()
        {
            if (availableElements.Count > 0)
            {
                return availableElements.Dequeue();
            }

            return null;
        }

        private void ReturnElementToPool(UserDataView element)
        {
            element.gameObject.SetActive(false);
            availableElements.Enqueue(element);
        }

        private void SetupElementPosition(UserDataView element, int index, UserData userData)
        {
            float yPos = -index * (elementHeight + spacing);
            element.RectTransform.anchoredPosition = new Vector2(0, yPos);
            element.gameObject.SetActive(true);
            element.UpdateElement(userData, false);
        }

        public void RefreshContentSize(int newTotalItems)
        {
            totalItems = newTotalItems;
            CalculateAndSetContentSize();
        }

        private void CalculateAndSetContentSize()
        {
            contentHeight = totalItems * (elementHeight + spacing);
            scrollViewContent.sizeDelta = new Vector2(0, contentHeight);
        }

        public void ClearAllElements()
        {
            foreach (var kvp in activeElements)
            {
                ReturnElementToPool(kvp.Value);
            }

            activeElements.Clear();

            if (stickyElement != null)
            {
                stickyElement.gameObject.SetActive(false);
                isShowingStickyElement = false;
            }
        }

        public float GetElementHeightWithSpacing()
        {
            return elementHeight + spacing;
        }
    }
}