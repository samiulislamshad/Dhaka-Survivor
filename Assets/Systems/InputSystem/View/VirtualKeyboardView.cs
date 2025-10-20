using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.InputSystem.View
{
    public class VirtualKeyboardView : MonoBehaviour
    {
        [SerializeField] private TMP_Text userNameText; // Text where the name appears
        [SerializeField] private int maxLength = 20;

        private readonly List<string> _specialKeys = new() {"Submit", "Cancel", "Delete"};

        private void Awake()
        {
            var buttons = GetComponentsInChildren<Button>(true);

            foreach (var btn in buttons)
            {
                var label = btn.GetComponentInChildren<TMP_Text>();

                if (label == null) continue;
                var key = label.text.Trim().ToUpperInvariant();

                // Filter out special buttons
                if (_specialKeys.Contains(key))
                {
                    switch (key)
                    {
                        case "SUBMIT": btn.onClick.AddListener(OnSubmit); break;
                        case "CANCEL": btn.onClick.AddListener(OnCancel); break;
                        case "DELETE": btn.onClick.AddListener(OnDelete); break;
                    }
                }
                else
                {
                    // Normal alphabet keys
                    btn.onClick.AddListener(() => OnLetterPressed(key));
                }
            }
        }

        private void OnLetterPressed(string letter)
        {
            if (userNameText.text.Length < maxLength)
                userNameText.text += letter;
        }

        private void OnDelete()
        {
            if (userNameText.text.Length > 0)
                userNameText.text = userNameText.text[..^1]; // remove last char
        }

        private void OnSubmit()
        {
            Debug.Log("Submitted name: " + userNameText.text);
            // Add your submit logic here (e.g., send to game manager or server)
        }

        private void OnCancel()
        {
            Debug.Log("Cancelled name input.");
            userNameText.text = "";
            // Add your cancel logic (e.g., hide keyboard UI)
        }
    }
}