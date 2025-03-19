using System;
using Core;
using Core.GameState;
using UnityEngine;
using YG;

namespace Services
{
    public class YandexGame : IService
    {
        
        private IGameManager gameManager;
        
        private void OnApplicationChangeState(ApplicationState state)
        {
            switch (state)
            {
                case ApplicationState.MainMenu:
                    
                    YG2.GameplayStop();
                    YG2.StickyAdActivity(true);
                    
                    break;
                
                case ApplicationState.Gameplay:
                    
                    YG2.GameplayStart();
                    YG2.StickyAdActivity(false);
                    
                    break;
                
                case ApplicationState.EndGame:
                    YG2.GameplayStop();
                    YG2.StickyAdActivity(true);
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        private void OnGameStateChanged(GameState state)
        {
            if (state == gameManager.CurrentGame.GameOver)
            {
                YG2.GameplayStop();
                YG2.StickyAdActivity(true);
            }
        }

        public void ShowInterAd()
        {
            YG2.InterstitialAdvShow();
        }
        
        #region IService

        private void OnServicesRegistered()
        {
            ServiceLocator.OnAllServicesRegistered -= OnServicesRegistered;
            gameManager = ServiceLocator.Get<IGameManager>();
            gameManager.OnGameStateChanged += OnApplicationChangeState;
            gameManager.CurrentGame.OnChangeState += OnGameStateChanged;
        }
        
        public void Initialize()
        {
            ServiceLocator.OnAllServicesRegistered += OnServicesRegistered;
            IsInitialized = true;
            Debug.Log("Yandex plugin initialized");
        }

        public void Shutdown()
        {
            gameManager.CurrentGame.OnChangeState -= OnGameStateChanged;
            gameManager.OnGameStateChanged -= OnApplicationChangeState;
            Debug.Log("Shutting down yandex plugin");
        }

        public bool IsInitialized { get; private set; }

        #endregion
        
    }
}