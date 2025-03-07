using UnityEngine;

namespace Services
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioManager : MonoBehaviour, IAudioService
    {
        [SerializeField] private PawnAudio pawnAudio;      // Клипы для шашек
        [SerializeField] private float volume; 

        private AudioSource audioSource;
        
        
        #region IService
        
        public void Initialize()
        {
            Debug.Log("Audio Manager initialized");
            audioSource = GetComponent<AudioSource>();
            pawnAudio.Initialize(audioSource);
            // todo загрузить громкость
            isInitialized = true;
        }

        public void Shutdown()
        {
            Debug.Log("Shutting down Audio Manager");
        }

        public bool isInitialized { get; private set; }
        
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
                
                // todo сохранить громкость
            }
        }
        
        // Доступ к клипам шашек
        public PawnAudio PawnAudio => pawnAudio;
        
        // Воспроизведение однократного звука (например, клик в меню)
        public void PlaySound(AudioClip clip)
        {
            audioSource.PlayOneShot(clip);
        }

        #endregion
    }
}