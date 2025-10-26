using Systems.SoundSystem.View;
using UnityEngine;
using Zenject;

namespace Systems.AudioSystem.Installer
{
    [CreateAssetMenu(fileName = "AudioInstaller", menuName = "Installers/AudioInstaller")]
    public class AudioInstaller : ScriptableObjectInstaller<AudioInstaller>
    {
        [SerializeField] private AudioView audioView;
    }
}