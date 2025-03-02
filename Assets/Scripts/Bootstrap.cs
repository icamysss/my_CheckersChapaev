using Services;
using UnityEngine;

public class Bootstrap : MonoBehaviour
{
    [SerializeField] private GameManager _gameManagerPrefab;
    [SerializeField] private UIManager _uiManagerPrefab;
    [SerializeField] private AudioManager _audioManagerPrefab;

    private void Awake()
    {
        InitializeCoreServices();
    }

    private void InitializeCoreServices()
    {
        // --------------------------------------------------
        //              GAME MANAGER
        // --------------------------------------------------
        var gameManager = Instantiate(_gameManagerPrefab);
        DontDestroyOnLoad(gameManager);
        ServiceLocator.Register<IGameManager>(gameManager);
        // --------------------------------------------------
        //              UI MANAGER
        // --------------------------------------------------
        var uiManager = Instantiate(_uiManagerPrefab);
        DontDestroyOnLoad(uiManager.gameObject);
        ServiceLocator.Register<IUIManager>(uiManager);
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
    
    private void OnApplicationQuit()
    {
        ServiceLocator.ResetAll();
    }
}