using System;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class PawnAudio
{
    public AudioClip[] movementClips;  // Клипы для движения шашки
    public AudioClip[] collideClips;   // Клипы для столкновения
    public AudioClip[] strikeClips;    // Клипы для удара
    
    private AudioSource _audioSource;

    public void Initialize(AudioSource source)
    {
        _audioSource = source;
    }
    
    // Воспроизведение однократного звука движения
    public void PlayMovementSound()
    {
        if (movementClips.Length > 0)
        {
            AudioClip clip = movementClips[Random.Range(0, movementClips.Length)];
            _audioSource.PlayOneShot(clip);
        }
    }

    // Начало зацикленного звука движения
    public void StartMovementLoop(AudioSource pawnAudioSource)
    {
        if (movementClips.Length > 0)
        {
            AudioClip clip = movementClips[Random.Range(0, movementClips.Length)];
            pawnAudioSource.clip = clip;
            pawnAudioSource.loop = true;
            pawnAudioSource.Play();
        }
    }

    // Остановка зацикленного звука
    public void StopMovementLoop(AudioSource pawnAudioSource)
    {
        pawnAudioSource.Stop();
    }

    // Воспроизведение звука столкновения
    public void PlayCollideSound()
    {
        if (collideClips.Length > 0)
        {
            AudioClip clip = collideClips[Random.Range(0, collideClips.Length)];
            _audioSource.PlayOneShot(clip);
        }
    }

    // Воспроизведение звука удара
    public void PlayStrikeSound()
    {
        if (strikeClips.Length > 0)
        {
            AudioClip clip = strikeClips[Random.Range(0, strikeClips.Length)];
            _audioSource.PlayOneShot(clip);
        }
    }
}