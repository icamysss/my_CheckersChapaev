using Services;
using UnityEngine;

public class Bootstrap : MonoBehaviour
{
    [SerializeField] private AudioManager _audioManagerPrefab;

    private void Awake()
    {
        InitializeCoreServices();
        SetupDependencies();
    }

    private void InitializeCoreServices()
    {
        var audioManager = Instantiate(_audioManagerPrefab);
        DontDestroyOnLoad(audioManager.gameObject);
        
        ServiceLocator.Register<IAudioService>(audioManager);
    }

    private void SetupDependencies()
    {
        // Настройка межсервисных зависимостей
    }

    private void OnApplicationQuit()
    {
        ServiceLocator.ResetAll();
    }
}