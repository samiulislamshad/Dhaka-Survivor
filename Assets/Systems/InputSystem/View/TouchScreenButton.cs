using System;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Systems.InputSystem.View
{
    public class TouchScreenButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private readonly Subject<Unit> _onButtonDown = new();
        private readonly Subject<Unit> _onButtonUp = new();
        
        public IObservable<Unit> OnButtonDown => _onButtonDown;
        public IObservable<Unit> OnButtonUp => _onButtonUp;
        
        public void OnPointerDown(PointerEventData eventData)
        {
            _onButtonDown.OnNext(Unit.Default);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _onButtonUp.OnNext(Unit.Default);
        }

        private void OnDestroy()
        {
            _onButtonDown.Dispose();
            _onButtonUp.Dispose();
        }
    }
}