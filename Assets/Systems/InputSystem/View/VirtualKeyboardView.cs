using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Systems.InputSystem.View
{
    public class VirtualKeyboardView : MonoBehaviour
    {
        public Button submitButton;
        public Button cancelButton;
        public Button deleteButton;

        public TMP_Text userNameText;

        [SerializeField] private Color buttonSelectedColor;
        [SerializeField] private EventSystem eventSystem;

        public void InitializeAlphanumericButtons(List<string> specialKeys, Action<string> callback)
        {
            if (callback == null)
            {
                Debug.LogError("Callback is null!");
                return;
            }
            
            var buttons = GetComponentsInChildren<Button>(true);

            foreach (var btn in buttons)
            {
                var label = btn.GetComponentInChildren<TMP_Text>();

                if (label == null) continue;
                var key = label.text;

                if (specialKeys.Contains(key)) continue;
                btn.onClick.AddListener(() => callback(key));
                var colors = btn.colors;
                colors.selectedColor = buttonSelectedColor;
                btn.colors = colors;
            }
            
            var buttonColors = submitButton.colors;
            buttonColors.selectedColor = buttonSelectedColor;
            submitButton.colors = buttonColors;
            cancelButton.colors = buttonColors;
            deleteButton.colors = buttonColors;
            
            if(eventSystem == null)
                eventSystem = FindFirstObjectByType<EventSystem>();
            eventSystem.SetSelectedGameObject(buttons[0].gameObject);
        }
    }
}