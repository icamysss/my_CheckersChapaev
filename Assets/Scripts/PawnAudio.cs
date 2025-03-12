using System;
using Services;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class PawnAudio
{
    public AudioClip[] movementClips;  // Клипы для движения шашки
    public AudioClip[] collideClips;   // Клипы для столкновения
    public AudioClip[] strikeClips;    // Клипы для удара
    
    private IAudioService audioService;

    public void Initialize(IAudioService audioManager)
    {
        audioService = audioManager;
    }
    
    // Воспроизведение однократного звука движения
    public void PlayMovementSound(AudioSource audioSource, float volume)
    {
        audioSource.volume = volume * audioService.Volume;
        var clip = movementClips[Random.Range(0, movementClips.Length)];
        if (clip != null && audioSource != null && !audioSource.isPlaying)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }

    public void StopMovementSound(AudioSource audioSource)
    {
        // Если шашка остановилась или оторвалась от доски
        if (audioSource.isPlaying)
        {
            // Плавно уменьшаем громкость перед остановкой
            if (audioSource.volume > 0.01f)
            {
                audioSource.volume = Mathf.Lerp(audioSource.volume, 0f, Time.deltaTime * 5);
            }
            else
            {
                audioSource.Stop(); // Останавливаем звук, когда громкость почти нулевая
                audioSource.volume = 0f;
            }
        }
    }

    // Воспроизведение звука столкновения
    public void PlayCollideSound(AudioSource audioSource)
    {
        if (collideClips.Length <= 0) return;
        
        var clip = collideClips[Random.Range(0, collideClips.Length)];
        audioSource.volume = audioService.Volume;
        audioSource.PlayOneShot(clip);
    }

    // Воспроизведение звука удара
    public void PlayStrikeSound(AudioSource audioSource)
    {
        if (strikeClips.Length <= 0) return;
        
        var clip = strikeClips[Random.Range(0, strikeClips.Length)];
        audioSource.volume = audioService.Volume;
        audioSource.PlayOneShot(clip);
    }
}