using System.Collections.Generic;
using Systems.GameSystem.Signals;
using UnityEngine;
using Zenject;

namespace Systems.AudioSystem.Manager
{
    public class AudioManager : MonoBehaviour
    {
        [Inject] private SignalBus _signalBus;

        [SerializeField] private List<AudioClip> mainMenuClip;
        [SerializeField] private AudioClip gameClip;
        
        [SerializeField] private AudioSource audioSource;

        private void Start()
        {
            audioSource.ignoreListenerPause = true;
            audioSource.ignoreListenerVolume = false;
            
            SubscribeToSignals();
            PlayMainMenuMusic();
        }

        private void SubscribeToSignals()
        {
            _signalBus.Subscribe<GameScreenSignal>(PlayGameScreenMusic);
        }
        
        private void UnsubscribeToSignals()
        {
            _signalBus.Unsubscribe<GameScreenSignal>(PlayGameScreenMusic);
        }

        private void PlayMainMenuMusic()
        {
            audioSource.clip = mainMenuClip[Random.Range(0, mainMenuClip.Count)];
            audioSource.Play();
        }
        
        private void PlayGameScreenMusic()
        {
            if(audioSource.isPlaying)
                audioSource.Stop();
            audioSource.clip = gameClip;
            audioSource.Play();
        }

        private void OnDestroy()
        {
            UnsubscribeToSignals();
        }
    }
}