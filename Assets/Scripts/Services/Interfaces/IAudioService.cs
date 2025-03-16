using UnityEngine;

namespace Services
{
    public interface IAudioService : IService
    {
        float Volume { get; set; } // 0.....1
        
        /// <summary>
        /// Доступ к звукам шашек
        /// </summary>
        PawnAudio PawnAudio { get; }
        
        void PlaySound(AudioClip clip);
        
        void Mute(bool isMute);
    }
}