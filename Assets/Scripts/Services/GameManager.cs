using System;
using System.Diagnostics;
using AI;
using Core;
using Services.Interfaces;
using Sirenix.OdinInspector;
using UnityEngine;
using YG;
using Debug = UnityEngine.Debug;

namespace Services
{
    public class GameManager : MonoBehaviour, IGameManager
    {
        [BoxGroup("References")]
        [SerializeField] private Board boardPrefab;
        [BoxGroup("References")]
        [SerializeField] private PawnCatcher pawnCatcherPrefab;
        [BoxGroup("Options")]
        [SerializeField] private ApplicationState applicationState = ApplicationState.MainMenu;
        public event Action<ApplicationState> OnGameStateChanged;
        
      
        
        private Board board;
        private Game game;
        
     
        
        private void OnDisable()
        {
            game.OnStart -= OnStartGame;
            game.OnEndGame -= OnEndGame;
        }

        private void SetApplicationState(ApplicationState newState)
        {
            // Debug.Log($"Game state changed to {newState}, old {applicationState}");
            //if (applicationState == newState) return;
            applicationState = newState;
            if (newState == ApplicationState.MainMenu) YG2.InterstitialAdvShow(); // BUG:  исправить вызов рекламы
            OnGameStateChanged?.Invoke(newState);
        }
        
        # region Game Events

        private void OnStartGame()
        {
            SetApplicationState(ApplicationState.Gameplay);
        }

        private void OnEndGame()
        {
            SetApplicationState(ApplicationState.EndGame);
        }
        
        
        
        #endregion
        
        #region IGameManager
        
        public ApplicationState CurrentState
        {
            get => applicationState;
            set => SetApplicationState(value);
        }
        public Game CurrentGame => game;

        #endregion
        
        #region IService

        private void OnAllServicesRegistered()
        {
            ServiceLocator.OnAllServicesRegistered -= OnAllServicesRegistered;
            // -------- Игра ------------
            game = new Game(this, board);
            game.OnStart += OnStartGame;
            game.OnEndGame += OnEndGame;
        }
        
        public void Initialize()
        {
            ServiceLocator.OnAllServicesRegistered += OnAllServicesRegistered;
            // -------- Ссылки --------
            if (boardPrefab == null) throw new NullReferenceException("boardPrefab is null");
            
            // -------- Окружение ------- 
            board = Instantiate(boardPrefab);
            var pawnCatcher = Instantiate(pawnCatcherPrefab);
            
            Debug.Log("Game Manager initialized");
            SetApplicationState(ApplicationState.MainMenu);
            IsInitialized = true;
        }

        public void Shutdown()
        {
            Debug.Log("Shutting down Game Manager");
        }
        
        public bool IsInitialized { get; private set; }

        #endregion 
    }
}