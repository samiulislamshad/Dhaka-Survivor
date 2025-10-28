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
    private float currentScrollPosition = 0f;
    
    void Start()
    {
        Initialize();
    }
    
    private void Initialize()
    {
        model.InitializeWithTestData();
        
        view.OnViewInitialized
            .Subscribe(_ => SetupRecyclableScrollView())
            .AddTo(disposables);
        
        view.Initialize(model.totalUserCount.Value, model.currentPlayerRank.Value);
    }
    
    private void SetupRecyclableScrollView()
    {
        StartCoroutine(SetupAfterLayout());
    }
    
    private System.Collections.IEnumerator SetupAfterLayout()
    {
        yield return new WaitForEndOfFrame();
        
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(view.scrollRect.viewport as RectTransform);
        
        float viewportHeight = view.scrollRect.viewport.rect.height;
        float elementHeightWithSpacing = view.GetElementHeightWithSpacing();
        
        if (viewportHeight <= 0)
        {
            viewportHeight = 600f;
            Debug.LogWarning($"Viewport height was 0, using fallback: {viewportHeight}");
        }
        
        visibleItemCount = Mathf.CeilToInt(viewportHeight / elementHeightWithSpacing) + 2;
        
        Debug.Log($"Recyclable Scroll View Setup:");
        Debug.Log($"- Viewport height: {viewportHeight}");
        Debug.Log($"- Element height + spacing: {elementHeightWithSpacing}");
        Debug.Log($"- Visible item count: {visibleItemCount}");
        Debug.Log($"- Current player rank: {model.currentPlayerRank.Value}");
        
        view.OnScrollValueChanged
            .Subscribe(OnScrollValueChanged)
            .AddTo(disposables);
        
        model.userDataList
            .Subscribe(OnUserDataChanged)
            .AddTo(disposables);
        
        UpdateVisibleElements(0);
    }
    
    private void OnScrollValueChanged(float scrollValue)
    {
        currentScrollPosition = scrollValue;
        
        if (model.userDataList.Value == null || model.userDataList.Value.Count == 0) return;
        
        float contentHeight = view.scrollViewContent.rect.height;
        float viewportHeight = view.scrollRect.viewport.rect.height;
        
        if (viewportHeight <= 0)
        {
            viewportHeight = (view.scrollRect.viewport as RectTransform).rect.height;
        }
        
        if (contentHeight <= viewportHeight) 
        {
            UpdateVisibleElements(0);
            return;
        }
        
        float scrollPos = (1f - scrollValue) * (contentHeight - viewportHeight);
        int newStartIndex = Mathf.FloorToInt(scrollPos / view.GetElementHeightWithSpacing());
        newStartIndex = Mathf.Max(0, newStartIndex - 1);
        
        UpdateVisibleElements(newStartIndex);
    }
    
    private void UpdateVisibleElements(int startIndex)
    {
        if (model.userDataList.Value == null) return;
        
        startIndex = Mathf.Clamp(startIndex, 0, model.userDataList.Value.Count - 1);
        int endIndex = Mathf.Clamp(startIndex + visibleItemCount - 1, 0, model.userDataList.Value.Count - 1);
        
        currentStartIndex = startIndex;
        currentEndIndex = endIndex;
        
        view.UpdateVisibleElements(startIndex, endIndex, model.userDataList.Value, currentScrollPosition);
    }
    
    private void OnUserDataChanged(List<UserData> userData)
    {
        if (userData == null) return;
        
        view.RefreshContentSize(userData.Count);
        UpdateVisibleElements(currentStartIndex);
    }
    
    void OnDestroy()
    {
        disposables.Dispose();
    }    }
}