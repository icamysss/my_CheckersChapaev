using System;
using AI;
using Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Services
{
    public class GameManager : MonoBehaviour, IGameManager
    {
        [BoxGroup("References")]
        [SerializeField] private Board boardPrefab;
        [BoxGroup("References")]
        [SerializeField] private AIController aiControllerPrefab;
        [BoxGroup("References")]
        [SerializeField] private PawnCatcher pawnCatcherPrefab;
        
        [BoxGroup("Options")]
        [SerializeField] private Game game;
        [BoxGroup("Options")]
        [SerializeField] private GameState gameState = GameState.MainMenu;
       
        
        
        private IUIManager uiManager;

        private void Update()
        {
            if (!isInitialized) return;
            game.GameUpdate();
        }

        private void SetGameState(GameState newState)
        {
            Debug.Log($"Game state changed to {newState}, old {gameState}");
            if (gameState == newState) return;

            gameState = newState;
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
        }
        
        #region IGameManager
        
        public event Action<GameState> OnGameStateChanged;
        public GameState CurrentState
        {
            get => gameState;
            set => SetGameState(value);
        }
        public Game CurrentGame => game;

        #endregion
        
        #region IService

        public void Initialize()
        {
            // -------- Ссылки --------
            if (boardPrefab == null) throw new NullReferenceException("boardPrefab is null");
            if (aiControllerPrefab == null) throw new NullReferenceException("aiControllerPrefab is null");


            // -------- Окружение ------- 
            var board = Instantiate(boardPrefab);
            var aiController = Instantiate(aiControllerPrefab);
            var pawnCatcher = Instantiate(pawnCatcherPrefab);
            // -------- Игра ------------
            game = new Game(this, board, aiController);


            ServiceLocator.OnAllServicesRegistered += OnServicesReady;
            Debug.Log("Game Manager initialized");
            isInitialized = true;
        }

        public void Shutdown()
        {
            Debug.Log("Shutting down Game Manager");
            game.Dispose();
        }
        
        public bool isInitialized { get; private set; }

        #endregion 
    }
}