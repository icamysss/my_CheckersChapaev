using System;
using Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Services
{
    public class GameManager : MonoBehaviour, IGameManager
    {
        [BoxGroup("References")]
        [SerializeField] private Board boardPrefab; // todo ссылку в гейминфо
        [BoxGroup("References")]
        [SerializeField] private AIController aiControllerPrefab;
        
        [BoxGroup("Options")]
        [SerializeField] private Game game;
        [BoxGroup("Options")]
        [SerializeField] private GameState gameState;
       
        
        
        private IUIManager uiManager;
        
        
        private void SetGameState(GameState newState)
        {
            if (CurrentState == newState) return;

            CurrentState = newState;
            OnGameStateChanged?.Invoke(newState);
        
            // Обработка специфичной логики состояний
            switch (newState)
            {
                case GameState.MainMenu:
                    Time.timeScale = 1;
                   // UIManager.Instance.OpenMenu<MainMenu>();
                    break;
                
                case GameState.Gameplay:
                    Time.timeScale = 1;
                    //UIManager.Instance.OpenMenu<InGameMenu>();
                    break;
                
                case GameState.Pause:
                    Time.timeScale = 0;
                    //UIManager.Instance.OpenMenu<PauseMenu>();
                    break;
                
                case GameState.GameOver:
                    Time.timeScale = 1;
                   // UIManager.Instance.OpenMenu<GameOverMenu>();
                    break;
            }
        }

        private void OnServicesReady()
        {
            uiManager = ServiceLocator.Get<IUIManager>();
            ServiceLocator.OnAllServicesRegistered -= OnServicesReady;
            
            SetGameState(GameState.MainMenu);
        }
        
        #region IGameManager
        
        public event Action<GameState> OnGameStateChanged;
        public GameState CurrentState
        {
            get => gameState;
            set
            {
                gameState = value;
                SetGameState(gameState);
            } 
        }
        public Game CurrentGame => game;

        #endregion
        
        #region IService

        public void Initialize()
        {
            if (boardPrefab == null) throw new NullReferenceException("boardPrefab is null");
            if (aiControllerPrefab == null) throw new NullReferenceException("aiControllerPrefab is null");
            
             var board = Instantiate(boardPrefab);
             game.Board = board;
             var aiController = Instantiate(aiControllerPrefab);
             game.AIController = aiController;

            
             
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