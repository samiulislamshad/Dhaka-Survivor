using System;
using UnityEngine;

namespace Systems.AudioSystem.Handler
{
    [Serializable]
    public class OneShotPlayer : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource;

        private void Awake()
        {
            audioSource.ignoreListenerPause = true;
        }

        public void PlayAudio()
        {
            if(audioSource.isPlaying) return;
            audioSource.Play();
        }

        public void StopAudio()
        {
            audioSource.Stop();
        }

        public void SetVolume(float volume) => audioSource.volume = volume;
    }
}