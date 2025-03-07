using UnityEngine;

namespace Services
{
    public interface IAudioService : IService
    {
        /// <summary>
        /// Устанавливает громкость
        /// </summary>
        /// <param name="volume">float от 0 до 1</param>
        float Volume { get; set; } // 0.....1
        
        /// <summary>
        /// Доступ к клипам звуков шашек
        /// </summary>
        PawnAudio PawnAudio { get; }
        
        
        /// <summary>
        /// Воспроизводит однократный звук эффекта
        /// </summary>
        void PlaySound(AudioClip clip);
    }
}