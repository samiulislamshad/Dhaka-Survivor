using Systems.InputSystem.Service;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.GameSystem.View
{
    public class StartGameCanvasView : MonoBehaviour
    {
        public GameObject gamepadKeyMappingPanel;
        public GameObject keyboardKeyMappingPanel;
        public GameObject touchScreenKeyMappingPanel;

        public Button keyboardStartButton;
        public Button touchScreenStartButton;

        public void ToggleKeyMapBasedOnInput(InputDeviceType deviceType)
        {
            switch (deviceType)
            {
                case InputDeviceType.KeyboardMouse:
                    keyboardKeyMappingPanel.SetActive(true);
                    gamepadKeyMappingPanel.SetActive(false);
                    touchScreenKeyMappingPanel.SetActive(false);
                    break;
                case InputDeviceType.Gamepad:
                    keyboardKeyMappingPanel.SetActive(false);
                    gamepadKeyMappingPanel.SetActive(true);
                    touchScreenKeyMappingPanel.SetActive(false);
                    break;
                case InputDeviceType.TouchScreen:
                    keyboardKeyMappingPanel.SetActive(false);
                    gamepadKeyMappingPanel.SetActive(false);
                    touchScreenKeyMappingPanel.SetActive(true);
                    break;
                default:
                    keyboardKeyMappingPanel.SetActive(true);
                    gamepadKeyMappingPanel.SetActive(false);
                    touchScreenKeyMappingPanel.SetActive(false);
                    break;
            }
        }

        public void ToggleKeymappingPanel(bool isVisible) => gameObject.SetActive(isVisible);
    }
}