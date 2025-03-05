using UnityEngine;

namespace Services
{
    public class AudioManager : MonoBehaviour, IAudioService
    {
        [SerializeField] private PawnAudio pawnAudio;      // Клипы для шашек
        [SerializeField] private AudioSource musicSource;  // Источник для фоновой музыки
        [SerializeField] private AudioSource sfxSource;    // Источник для звуков эффектов

        #region IService
        
        public void Initialize()
        {
            Debug.Log("Audio Manager initialized");
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
        public void SetVolume(float volume)
        {
            musicSource.volume = volume;
            sfxSource.volume = volume;
        }

        // Доступ к клипам шашек
        public PawnAudio PawnAudio => pawnAudio;

        // Воспроизведение фоновой музыки
        public void PlayMusic(AudioClip clip)
        {
            musicSource.clip = clip;
            musicSource.loop = true;
            musicSource.Play();
        }

        // Остановка музыки
        public void StopMusic()
        {
            musicSource.Stop();
        }

        // Воспроизведение однократного звука (например, клик в меню)
        public void PlaySound(AudioClip clip)
        {
            sfxSource.PlayOneShot(clip);
        }

        #endregion
    }
}