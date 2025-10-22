using System;
using Systems.PauseSystem.Signals;
using UniRx;
using UnityEngine;
using Zenject;

namespace Systems.PauseSystem.Controller
{
    [Serializable]
    public class PauseController : IDisposable
    {
        private SignalBus _signalBus;
        private CompositeDisposable _disposable;

        public PauseController(SignalBus signalBus)
        {
            _signalBus = signalBus;
            _disposable = new CompositeDisposable();

            SubscribeToSignals();
        }

        private void SubscribeToSignals()
        {
            _signalBus.Subscribe<PauseSignal>(PauseGame);
            _signalBus.Subscribe<UnpauseSignal>(UnpauseGame);
        }

        private void PauseGame()
        {
            Time.timeScale = 0;
        }

        private void UnpauseGame()
        {
            Time.timeScale = 1;
        }
        
        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}