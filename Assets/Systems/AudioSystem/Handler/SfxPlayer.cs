using System;
using UnityEngine;

namespace Systems.AudioSystem.Handler
{
    public class SfxPlayer : MonoBehaviour
    {
        [SerializeField] private AudioClip sfxClip;
        private AudioSource _audioSource;

        private void Awake()
        {
            _audioSource = gameObject.GetComponent<AudioSource>();
            _audioSource.clip = sfxClip;
            _audioSource.playOnAwake = false;
        }
        
        public bool IsPlaying() => _audioSource.isPlaying;

        public void PlaySfx()
        {
            if(_audioSource.isPlaying) return;
            _audioSource.Play();
        }

        public void StopSfx() => _audioSource.Stop();
        public void SetVolume(float volume) => _audioSource.volume = volume;
    }
}