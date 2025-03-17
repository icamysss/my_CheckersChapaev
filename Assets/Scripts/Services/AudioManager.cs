using UnityEngine;

namespace Services
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioManager : MonoBehaviour, IAudioService
    {
        [SerializeField] private PawnAudio pawnAudio;      // Клипы для шашек
        [SerializeField] private float volume; 

        private AudioSource audioSource;
        private float lastVolume;
        
        
        #region IService
        
        public void Initialize()
        {
            Debug.Log("Audio Manager initialized");
            audioSource = GetComponent<AudioSource>();
            volume = 0.6f;
            pawnAudio.Initialize(this);
            IsInitialized = true;
        }

        public void Shutdown()
        {
            Debug.Log("Shutting down Audio Manager");
        }

        public bool IsInitialized { get; private set; }
        
        #endregion

        #region IAudioService

        // Установка общей громкости
        public float Volume
        {
            get => volume;
            set
            {
                value = Mathf.Clamp(value, 0, 1);
                audioSource.volume = volume;
                volume = value;
            }
        }
        
        // Доступ к клипам шашек
        public PawnAudio PawnAudio => pawnAudio;
        
        // Воспроизведение однократного звука (например, клик в меню)
        public void PlaySound(AudioClip clip)
        {
            if (volume <= 0) return;
            audioSource.volume = volume;
            audioSource.PlayOneShot(clip);
        }

        public void Mute(bool isMute)
        {
            if (isMute && volume > 0)  lastVolume = volume;
           
            volume = isMute ? 0 : lastVolume;
        }

        #endregion
    }
}