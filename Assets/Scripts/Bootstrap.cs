using Services;
using UnityEngine;

public class Bootstrap : MonoBehaviour
{
    [SerializeField] private AudioManager _audioManagerPrefab;
    [SerializeField] private GameManager _gameManagerPrefab;
    [SerializeField] private UIManager _uiManagerPrefab;

    private void Awake()
    {
        InitializeCoreServices();
        SetupDependencies();
    }

    private void InitializeCoreServices()
    {
        // --------------------------------------------------
        //              UI MANAGER
        // --------------------------------------------------
        var uiManager = Instantiate(_uiManagerPrefab);
        DontDestroyOnLoad(uiManager.gameObject);
        ServiceLocator.Register<IUIManager>(uiManager);
        // --------------------------------------------------
        //              GAME MANAGER
        // --------------------------------------------------
        var gameManager = Instantiate(_gameManagerPrefab);
        DontDestroyOnLoad(gameManager);
        ServiceLocator.Register<IGameManager>(gameManager);
        // --------------------------------------------------
        //              AUDIO MANAGER
        // --------------------------------------------------
        var audioManager = Instantiate(_audioManagerPrefab);
        DontDestroyOnLoad(audioManager.gameObject);
        ServiceLocator.Register<IAudioService>(audioManager);
        // --------------------------------------------------
        
        // все сервисы отправлены в локатор
        ServiceLocator.AllServicesRegistered = true;
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