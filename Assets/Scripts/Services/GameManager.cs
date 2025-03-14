using System;
using AI;
using Core;
using Services.Interfaces;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Services
{
    // todo связать состояние игры с состоянием приложения
    // gameManager.CurrentState = ApplicationState.Gameplay;
    
    
    
    public class GameManager : MonoBehaviour, IGameManager
    {
        [BoxGroup("References")]
        [SerializeField] private Board boardPrefab;
        [BoxGroup("References")]
        [SerializeField] private PawnCatcher pawnCatcherPrefab;
        
        [BoxGroup("Options")]
        [SerializeField] private Game game;
        [BoxGroup("Options")]
        [SerializeField] private ApplicationState applicationState = ApplicationState.MainMenu;
       
        
        
        private IUIManager uiManager;
        
        private void SetGameState(ApplicationState newState)
        {
            Debug.Log($"Game state changed to {newState}, old {applicationState}");
            if (applicationState == newState) return;

            applicationState = newState;
            OnGameStateChanged?.Invoke(newState);
        
            // Обработка специфичной логики состояний
            switch (newState)
            {
                case ApplicationState.MainMenu:
                    Time.timeScale = 1;
                    break;
                
                case ApplicationState.Gameplay:
                    Time.timeScale = 1;
                    break;
                
                case ApplicationState.Pause:
                    Time.timeScale = 0;
                    break;
                
                case ApplicationState.GameOver:
                    Time.timeScale = 1;
                    break;
            }
        }

        private void OnServicesReady()
        {
            uiManager = ServiceLocator.Get<IUIManager>();
            ServiceLocator.OnAllServicesRegistered -= OnServicesReady;
        }
        
        #region IGameManager
        
        public event Action<ApplicationState> OnGameStateChanged;
        public ApplicationState CurrentState
        {
            get => applicationState;
            set => SetGameState(value);
        }
        public Game CurrentGame => game;

        #endregion
        
        #region IService

        public void Initialize()
        {
            // -------- Ссылки --------
            if (boardPrefab == null) throw new NullReferenceException("boardPrefab is null");


            // -------- Окружение ------- 
            var board = Instantiate(boardPrefab);
            var pawnCatcher = Instantiate(pawnCatcherPrefab);
            // -------- Игра ------------
            game = new Game(this, board);


            ServiceLocator.OnAllServicesRegistered += OnServicesReady;
            Debug.Log("Game Manager initialized");
            isInitialized = true;
        }

        public void Shutdown()
        {
            Debug.Log("Shutting down Game Manager");
        }
        
        public bool isInitialized { get; private set; }

        #endregion 
    }
}