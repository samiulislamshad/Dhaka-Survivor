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
[Header("References")]
    public RectTransform scrollViewContent;
    public ScrollRect scrollRect;
    public GameObject elementPrefab;
    
    [Header("Settings")]
    public int poolSize = 20;
    public float spacing = 5f;
    public float elementHeight = 100f;
    
    private List<UserDataView> elementPool = new List<UserDataView>();
    private Queue<UserDataView> availableElements = new Queue<UserDataView>();
    private Dictionary<int, UserDataView> activeElements = new Dictionary<int, UserDataView>();
    
    // Events
    public Subject<Unit> onViewInitialized = new Subject<Unit>();
    public Subject<float> onScrollValueChanged = new Subject<float>();
    
    private int totalItems;
    private float contentHeight;

    void Awake()
    {
        // Remove any layout components that might interfere
        RemoveLayoutComponents();
    }
    
    public void Initialize(int totalItems)
    {
        this.totalItems = totalItems;
        
        RemoveLayoutComponents();
        ConfigureScrollRect();
        CreateElementPool();
        SetupScrollContent();
        
        Debug.Log($"LeaderBoardView initialized with {totalItems} items, pool size: {poolSize}");
        
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
        
        Debug.Log($"Created element pool with {elementPool.Count} elements");
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
        
        Debug.Log($"Content size: {scrollViewContent.sizeDelta}, Viewport: {scrollRect.viewport.rect.size}");
        
        scrollRect.onValueChanged.AsObservable()
            .Subscribe(value => onScrollValueChanged.OnNext(value.y))
            .AddTo(this);
    }
    
    private void CalculateAndSetContentSize()
    {
        contentHeight = totalItems * (elementHeight + spacing);
        scrollViewContent.sizeDelta = new Vector2(0, contentHeight);
    }
    
    public void UpdateVisibleElements(int startIndex, int endIndex, List<UserData> userData)
    {
        if (userData == null)
        {
            Debug.LogError("Cannot update visible elements: userData is null");
            return;
        }
        
        Debug.Log($"UpdateVisibleElements called: {startIndex} to {endIndex}, Available elements: {availableElements.Count}");
        
        // Return elements that are no longer visible
        var keysToRemove = activeElements.Keys.Where(key => key < startIndex || key > endIndex).ToList();
        foreach (var key in keysToRemove)
        {
            ReturnElementToPool(activeElements[key]);
            activeElements.Remove(key);
        }
        
        Debug.Log($"Returned {keysToRemove.Count} elements to pool. Now available: {availableElements.Count}");
        
        // Get elements for visible indices
        int elementsCreated = 0;
        for (int i = startIndex; i <= endIndex; i++)
        {
            if (i >= 0 && i < userData.Count && !activeElements.ContainsKey(i))
            {
                var element = GetElementFromPool();
                if (element != null)
                {
                    SetupElementPosition(element, i, userData[i]);
                    activeElements[i] = element;
                    elementsCreated++;
                }
            }
        }
        
        Debug.Log($"Created {elementsCreated} new elements. Total active: {activeElements.Count}");
    }
    
    private UserDataView GetElementFromPool()
    {
        if (availableElements.Count > 0)
        {
            return availableElements.Dequeue();
        }
        
        Debug.LogWarning("No available elements in pool!");
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
        element.UpdateElement(userData);
        
        Debug.Log($"Element {index} positioned at Y: {yPos}");
    }
    
    public void RefreshContentSize(int newTotalItems)
    {
        totalItems = newTotalItems;
        CalculateAndSetContentSize();
        Debug.Log($"Content size refreshed: {scrollViewContent.sizeDelta}");
    }
    
    public void ClearAllElements()
    {
        foreach (var kvp in activeElements)
        {
            ReturnElementToPool(kvp.Value);
        }
        activeElements.Clear();
    }
    
    public float GetElementHeightWithSpacing()
    {
        return elementHeight + spacing;
    }    }
}