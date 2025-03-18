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
        [SerializeField] private List<Menu> menuPrefabs = new ();

        private Transform uiRoot; // кэш своего трансформ
        private readonly Dictionary<string, Menu> InstantiatedMenus = new ();

        public IGameManager GameManager { get; private set; }
        public IAudioService AudioService { get; private set; }
        public ILocalizationService LocalizationService { get; private set; }
        public YandexPlugin YandexPlugin { get; private set; }
        
        private void OnDisable()
        {
            GameManager.OnGameStateChanged -= OnChangeApplicationState;
        }

        private void InitializeMenus()
        {
            foreach (var menuPrefab in menuPrefabs)
            {
                var type = menuPrefab.MenuType;
                var menu = Instantiate(menuPrefab, uiRoot);
                menu.Hide();
                menu.Initialize(this);
               
                if (!InstantiatedMenus.TryAdd(type, menu))
                {
                    Debug.LogError($"Duplicate menu type: {type}");
                }
            }
        }
        
        private void OnChangeApplicationState(ApplicationState state)
        {
            switch (state)
            {
                case ApplicationState.MainMenu:
                    CloseAllMenus();
                    ShowMenu("MainMenu");
                    break;
                case ApplicationState.Gameplay:
                    CloseAllMenus();
                    ShowMenu("InGame");
                    break;
                case ApplicationState.ShowingAD:
                    CloseAllMenus();
                    break;
                case ApplicationState.EndGame:
                    CloseAllMenus();
                    ShowMenu("EndGame");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }
        
        private void OnServicesReady()
        {
            // ----- ссылки -----
            GameManager = ServiceLocator.Get<IGameManager>();
            AudioService = ServiceLocator.Get<IAudioService>();
            LocalizationService = ServiceLocator.Get<ILocalizationService>();
            YandexPlugin = ServiceLocator.Get<YandexPlugin>();
            GameManager.OnGameStateChanged += OnChangeApplicationState;
            ServiceLocator.OnAllServicesRegistered -= OnServicesReady;
            // ---- меню ----
            InitializeMenus();
            ShowMenu("MainMenu");
        }
        
        #region IService

        public void Initialize()
        {
            if (IsInitialized) return;
            uiRoot = transform;
            ServiceLocator.OnAllServicesRegistered += OnServicesReady;
            Debug.Log("UIManager initialized");
            
            IsInitialized = true;
        }
        
        public void Shutdown()
        {
            Debug.Log("UIManager shutting down");
        }

        public bool IsInitialized { get; private set; }

        #endregion

        #region IUIManager

        public void ShowMenu(string menuType)
        {
            if (InstantiatedMenus.TryGetValue(menuType, out var menu))
            {
                menu.Show();
            }
            else Debug.LogError($"Could not find menu type: {menuType}");
        }

        public void HideMenu(string menuType)
        {
            if (InstantiatedMenus.TryGetValue(menuType, out var menu))
            {
                menu.Hide();
            }
            else Debug.LogError($"Could not find menu type: {menuType}");
        }

        public void CloseAllMenus()
        {
            foreach (var menu in InstantiatedMenus.Values)
            {
               menu.Hide();
            }
        }

        #endregion
    }
}