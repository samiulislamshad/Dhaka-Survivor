using UnityEngine;
using UnityEngine.UI;

namespace Systems.LoadingScreenSystem.View
{
    public class LoadingScreenCanvasView : MonoBehaviour
    {
        public GameObject loadingScreen;
        public Slider loadingSlider;

        public void SetSliderValue(float value)
        {
            loadingSlider.value = value;
        }
    }
}