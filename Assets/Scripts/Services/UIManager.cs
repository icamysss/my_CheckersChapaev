using System;
using System.Collections.Generic;
using Core;
using Services.Interfaces;
using UI;
using UnityEngine;

namespace Services
{
    public class UIManager : MonoBehaviour, IUIManager
    {
        [SerializeField] private List<Menu> _menuPrefabs = new ();

        private Transform _uiRoot; // кэш своего трансформ
        private Stack<Menu> _menuStack = new ();
        private Dictionary<string, Menu> _prefabCache = new ();
        private Dictionary<string, Menu> _activeMenus = new ();

        private IGameManager gameManager;
        private IAudioService audioService;

        private void OnChangeGameState(GameState state)
        {
            switch (state)
            {
                case GameState.MainMenu:
                    CloseAllMenus();
                    OpenMenu("MainMenu");
                    break;
                case GameState.Gameplay:
                    CloseMenu("MainMenu");
                    OpenMenu("InGame");
                    break;
                case GameState.Pause:
                    break;
                case GameState.GameOver:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }
        private void InitializePrefabCache()
        {
            foreach (var menuPrefab in _menuPrefabs)
            {
                var type = menuPrefab.MenuType;

                if (!_prefabCache.TryAdd(type, menuPrefab))
                {
                    Debug.LogError($"Duplicate menu type: {type}");
                    continue;
                }
            }
        }

        private void BringToFront(Menu menu)
        {
            menu.transform.SetAsLastSibling();
            menu.Open();
        }

        private void OnServicesReady()
        {
            // ----- ссылки -----
            gameManager = ServiceLocator.Get<IGameManager>();
            audioService = ServiceLocator.Get<IAudioService>();
            gameManager.OnGameStateChanged += OnChangeGameState;
            ServiceLocator.OnAllServicesRegistered -= OnServicesReady;
            // ---- меню ----
            OpenMenu("MainMenu");
            
        }

        public float GetVolume()
        {
            return audioService.Volume;
        }

        public string GetLanguage()
        {
            Debug.Log("Всегда русский !!!");
            return "ru";
            // todo получить язык
        }

        #region ButtonsHandlers

        public void StartGame(GameType gameType)
        {
            gameManager.CurrentGame.StartGame(gameType);
        }

        public void RestartGame()
        {
            
        }

        public void OpenMainMenu()
        {
            
        }

        public void SwitchSound(bool isOn)
        {
            
        }

        public void SetVolume(float volume)
        {
            
        }

        public void SetLanguage(string language)
        {
            
        }
        #endregion
        
        #region IService

        public void Initialize()
        {
            ServiceLocator.OnAllServicesRegistered += OnServicesReady;
            Debug.Log("UIManager initialized");
            InitializePrefabCache();
            isInitialized = true;
        }
        
        public void Shutdown()
        {
            Debug.Log("UIManager shutting down");
            gameManager.OnGameStateChanged -= OnChangeGameState;
        }

        public bool isInitialized { get; private set; }

        #endregion

        #region IUIManager

        public void OpenMenu(string menuType)
        {
            if (_prefabCache.TryGetValue(menuType, out Menu prefab))
            {
                // Если меню уже открыто
                if (_activeMenus.TryGetValue(menuType, out Menu existingMenu))
                {
                    BringToFront(existingMenu);
                    return;
                }

                // Создаем новый экземпляр
                var instance = Instantiate(prefab, _uiRoot);
                instance.Initialize(this);
                instance.Open();

                _menuStack.Push(instance);
                _activeMenus.Add(menuType, instance);
            }
            else Debug.LogError($"Could not find menu type: {menuType}");
        }

        public void CloseMenu(string menuType)
        {
            if (_activeMenus.TryGetValue(menuType, out Menu menu))
            {
                // Удаляем из стека
                var newStack = new Stack<Menu>();
                while (_menuStack.Count > 0)
                {
                    var item = _menuStack.Pop();
                    if (item != menu) newStack.Push(item);
                }
                _menuStack = newStack;

                // Уничтожаем меню
                menu.Close();
                Destroy(menu.gameObject);
                _activeMenus.Remove(menuType);

                // Восстанавливаем порядок
                if (_menuStack.Count > 0)
                {
                    BringToFront(_menuStack.Peek());
                }
            }else Debug.LogError($"Could not find menu type in active menus: {menuType}");
        }

        public void CloseTopMenu()
        {
            if (_menuStack.Count == 0) return;

            var menu = _menuStack.Pop();
            _activeMenus.Remove(menu.MenuType);
            menu.Close();
            
            if (_menuStack.Count > 0)
            {
                BringToFront(_menuStack.Peek());
            }
        }

        public void CloseAllMenus()
        {
            _menuStack.Clear();
            foreach (var menu in _activeMenus.Values)
            {
                menu.Close();
            }
            _activeMenus.Clear();
        }

        #endregion
    }
}