using UnityEngine;

namespace Services
{
    public interface IAudioService : IService
    {
        /// <summary>
        /// Устанавливает громкость
        /// </summary>
        /// <param name="volume">float от 0 до 1</param>
        void SetVolume(float volume); // 0.....1
        
        /// <summary>
        /// Доступ к клипам звуков шашек
        /// </summary>
        PawnAudio PawnAudio { get; }
        
        /// <summary>
        /// Воспроизводит фоновую музыку
        /// </summary>
        void PlayMusic(AudioClip clip);
        
        /// <summary>
        /// Останавливает фоновую музыку
        /// </summary>
        void StopMusic();
        
        /// <summary>
        /// Воспроизводит однократный звук эффекта
        /// </summary>
        void PlaySound(AudioClip clip);
    }
}