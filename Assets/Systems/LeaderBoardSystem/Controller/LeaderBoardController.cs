using System;
using System.Collections.Generic;
using Systems.InputSystem.Model;
using Systems.LeaderBoardSystem.Model;
using Systems.LeaderBoardSystem.View;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.LeaderBoardSystem.Controller
{
    [Serializable]
    public class LeaderBoardController : MonoBehaviour
    {
[Header("Dependencies")]
    public LeaderBoardCanvasView view;
    
    private LeaderBoardModel model = new LeaderBoardModel();
    private CompositeDisposable disposables = new CompositeDisposable();
    
    private int visibleItemCount;
    private int currentStartIndex = 0;
    private int currentEndIndex = 0;
    
    void Start()
    {
        Initialize();
    }
    
    private void Initialize()
    {
        // Initialize model with test data
        model.InitializeWithTestData();
        
        // Wait for view to be initialized
        view.onViewInitialized
            .Subscribe(_ => SetupRecyclableScrollView())
            .AddTo(disposables);
        
        // Initialize view
        view.Initialize(model.totalUserCount.Value);
    }
    
    private void SetupRecyclableScrollView()
    {
        // Use Coroutine to wait for layout calculation
        StartCoroutine(SetupAfterLayout());
    }
    
    private System.Collections.IEnumerator SetupAfterLayout()
    {
        // Wait for end of frame to ensure layout is calculated
        yield return new WaitForEndOfFrame();
        
        // Force layout rebuild
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(view.scrollRect.viewport as RectTransform);
        
        // Now calculate visible items with proper viewport height
        float viewportHeight = view.scrollRect.viewport.rect.height;
        float elementHeightWithSpacing = view.GetElementHeightWithSpacing();
        
        // If viewport height is still 0, use a default fallback
        if (viewportHeight <= 0)
        {
            viewportHeight = 600f; // Fallback height
            Debug.LogWarning($"Viewport height was 0, using fallback: {viewportHeight}");
        }
        
        visibleItemCount = Mathf.CeilToInt(viewportHeight / elementHeightWithSpacing) + 2;
        
        Debug.Log($"Recyclable Scroll View Setup:");
        Debug.Log($"- Viewport height: {viewportHeight}");
        Debug.Log($"- Element height + spacing: {elementHeightWithSpacing}");
        Debug.Log($"- Visible item count: {visibleItemCount}");
        Debug.Log($"- Total items: {model.totalUserCount.Value}");
        
        // Subscribe to scroll events
        view.onScrollValueChanged
            .Subscribe(OnScrollValueChanged)
            .AddTo(disposables);
        
        // Subscribe to data changes
        model.userDataList
            .Subscribe(OnUserDataChanged)
            .AddTo(disposables);
        
        // Initial update - show first batch of elements
        UpdateVisibleElements(0);
    }
    
    private void OnScrollValueChanged(float scrollValue)
    {
        if (model.userDataList.Value == null || model.userDataList.Value.Count == 0) return;
        
        float contentHeight = view.scrollViewContent.rect.height;
        float viewportHeight = view.scrollRect.viewport.rect.height;
        
        // Recalculate viewport height if needed
        if (viewportHeight <= 0)
        {
            viewportHeight = (view.scrollRect.viewport as RectTransform).rect.height;
        }
        
        if (contentHeight <= viewportHeight) 
        {
            // All items fit in viewport
            UpdateVisibleElements(0);
            return;
        }
        
        // Calculate visible range based on scroll position
        float scrollPosition = (1f - scrollValue) * (contentHeight - viewportHeight);
        int newStartIndex = Mathf.FloorToInt(scrollPosition / view.GetElementHeightWithSpacing());
        
        // Add buffer for smoother scrolling
        newStartIndex = Mathf.Max(0, newStartIndex - 1);
        
        UpdateVisibleElements(newStartIndex);
    }
    
    private void UpdateVisibleElements(int startIndex)
    {
        if (model.userDataList.Value == null) 
        {
            Debug.LogWarning("User data list is null!");
            return;
        }
        
        startIndex = Mathf.Clamp(startIndex, 0, model.userDataList.Value.Count - 1);
        int endIndex = Mathf.Clamp(startIndex + visibleItemCount - 1, 0, model.userDataList.Value.Count - 1);
        
        Debug.Log($"Updating visible elements: {startIndex} to {endIndex}, Total active: {endIndex - startIndex + 1}");
        
        // Always update to ensure elements are properly positioned
        currentStartIndex = startIndex;
        currentEndIndex = endIndex;
        
        view.UpdateVisibleElements(startIndex, endIndex, model.userDataList.Value);
    }
    
    private void OnUserDataChanged(List<UserData> userData)
    {
        if (userData == null)
        {
            Debug.LogError("User data is null!");
            return;
        }
        
        Debug.Log($"User data updated: {userData.Count} items");
        view.RefreshContentSize(userData.Count);
        UpdateVisibleElements(currentStartIndex);
    }
    
    void OnDestroy()
    {
        disposables.Dispose();
    }    }
}