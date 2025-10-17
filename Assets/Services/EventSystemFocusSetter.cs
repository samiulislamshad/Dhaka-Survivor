using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Services
{
    public class EventSystemFocusSetter : MonoBehaviour
    {
        [SerializeField] private Button focusButton;

        private void Start()
        {
            EventSystem.current.SetSelectedGameObject(null);
            if(focusButton == null) return;
            EventSystem.current.SetSelectedGameObject(focusButton.gameObject);
        }
    }
}