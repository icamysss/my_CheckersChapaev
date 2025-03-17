using Services.Interfaces;
using UnityEngine;

namespace Services
{
    public class Bootstrap : MonoBehaviour
    {
        [SerializeField] private GameManager _gameManagerPrefab;
        [SerializeField] private UIManager _uiManagerPrefab;
        [SerializeField] private AudioManager _audioManagerPrefab;
        [SerializeField] private CameraController _cameraControllerPrefab;

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
            ServiceLocator.Register<IGameManager>(gameManager);
            // --------------------------------------------------
            //              UI MANAGER
            // --------------------------------------------------
            var uiManager = Instantiate(_uiManagerPrefab);
            ServiceLocator.Register<IUIManager>(uiManager);
            // --------------------------------------------------
            //              AUDIO MANAGER
            // --------------------------------------------------
            var audioManager = Instantiate(_audioManagerPrefab);
            ServiceLocator.Register<IAudioService>(audioManager);
            // --------------------------------------------------
            //              CAMERA CONTROLLER
            // --------------------------------------------------
            var cameraController = Instantiate(_cameraControllerPrefab);
            ServiceLocator.Register<ICameraController>(cameraController);
            // --------------------------------------------------
            //              LOCALIZATION
            // --------------------------------------------------
            var localizationService = new LocalizationService();
            ServiceLocator.Register<ILocalizationService>(localizationService);
            // --------------------------------------------------
            //              YANDEXPLUGIN  - для работы с яндекс плагином
            // --------------------------------------------------
            var yandexPlugin = new YandexPlugin();
            ServiceLocator.Register<YandexPlugin>(yandexPlugin);
            // --------------------------------------------------
            
            // все сервисы отправлены в локатор
            ServiceLocator.AllServicesRegistered = true;
        }
    
        private void OnApplicationQuit()
        {
            ServiceLocator.ResetAll();
        }
    }
}