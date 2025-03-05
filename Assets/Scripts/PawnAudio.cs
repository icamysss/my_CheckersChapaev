using System;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class PawnAudio
{
    public AudioClip[] movementClips;  // Клипы для движения шашки
    public AudioClip[] collideClips;   // Клипы для столкновения
    public AudioClip[] strikeClips;    // Клипы для удара
    
    // Воспроизведение однократного звука движения
    public void PlayMovementSound(AudioSource audioSource)
    {
        if (movementClips.Length > 0)
        {
            AudioClip clip = movementClips[Random.Range(0, movementClips.Length)];
            audioSource.PlayOneShot(clip);
        }
    }

    // Начало зацикленного звука движения
    public void StartMovementLoop(AudioSource audioSource)
    {
        if (movementClips.Length > 0)
        {
            AudioClip clip = movementClips[Random.Range(0, movementClips.Length)];
            audioSource.clip = clip;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    // Остановка зацикленного звука
    public void StopMovementLoop(AudioSource audioSource)
    {
        audioSource.Stop();
    }

    // Воспроизведение звука столкновения
    public void PlayCollideSound(AudioSource audioSource)
    {
        if (collideClips.Length > 0)
        {
            AudioClip clip = collideClips[Random.Range(0, collideClips.Length)];
            audioSource.PlayOneShot(clip);
        }
    }

    // Воспроизведение звука удара
    public void PlayStrikeSound(AudioSource audioSource)
    {
        if (strikeClips.Length > 0)
        {
            AudioClip clip = strikeClips[Random.Range(0, strikeClips.Length)];
            audioSource.PlayOneShot(clip);
        }
    }
}