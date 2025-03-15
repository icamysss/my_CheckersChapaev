using System;
using System.Threading;
using AI;
using Core.GameState;
using Cysharp.Threading.Tasks;
using Services;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Core
{
    public class Game 
    {
        private AIController AIController { get; }
        private readonly GameManager gameManager;

        #region StateMachine variables

        public FirstTurn FirstTurn;
        public AITurn AIMove ;
        public HumanTurn HumanMove ;
        public EndGame GameOver ;
        private GameState.GameState currentState;
        #endregion

        #region Public properties
        public Board Board { get; }
        public GameType GameType { get; private set; } = GameType.HumanVsAi;
        public Player Winner { get; private set; }// Результат игры (победитель или ничья)
        public Player CurrentTurn { get; private set; } // Текущий ход ( игрока)
        public Player FirstPlayer { get; private set; }
        public Player SecondPlayer { get; private set; }

        public GameState.GameState CurrentState
        {
            get => currentState;
            private set
            {
                if (value == null)
                {
                    Debug.LogWarning("Value is null, nothing changed");
                    return;
                }
                currentState = value;
            }
        }
        
        #endregion
        
        /// <summary>
        /// Конструктор игры, инициализирует зависимости и подписывается на события шашек.
        /// </summary>
        public Game(GameManager gameManager, Board board)
        {
            this.gameManager = gameManager;
            Board = board;

            this.AIController = new AIController();
            this.AIController.Initialize(this);
            InitializeStateMachine();
        }

        public void ChangeState(GameState.GameState newState)
        {
            CurrentState?.Exit();
            CurrentState = IsGameOver() ? GameOver : newState;
            CurrentState.Enter();
        }

        #region GameEvents

        public Action OnStart;
        public Action OnEndTurn;
        public Action OnStartTurn;
        public Action OnEndGame;
        public Action<GameState.GameState> OnChangeState;

        #endregion

        #region GameCircle

        /// <summary>
        /// Запускает игру: настраивает доску и определяет, кто ходит первым.
        /// </summary>
        public void StartGame(GameType gameType)
        {
            GameType = gameType;
            ChangeState(FirstTurn);
        }

        /// <summary>
        /// Проверяет, закончилась ли игра, и определяет победителя.
        /// </summary>
        public bool IsGameOver()
        {
            if (CurrentState == FirstTurn 
                || CurrentState == GameOver 
                || CurrentState == null) 
                return false;
            
            var black = Board.GetPawnsOnBoard(PawnColor.Black);
            var white = Board.GetPawnsOnBoard(PawnColor.White);

            switch (black.Count)
            {
                case 0 when white.Count == 0:
                    Winner = null;
                    gameManager.CurrentState = ApplicationState.GameOver;
                    return true;
                // Белые победили
                case 0:
                    Winner = GetOppositePlayer(PawnColor.White);
                    gameManager.CurrentState = ApplicationState.GameOver;
                    return true;
            }

            // Черные победили
            if (white.Count == 0)
            {
                Winner = GetOppositePlayer(PawnColor.Black);
                gameManager.CurrentState = ApplicationState.GameOver;
                return true;
            }
            // Игра продолжается
            Winner = null;
            return false;
        }
        
        #endregion

        #region Helpers

        /// <summary>
        /// Обновляет интерактивность шашек текущего игрока.
        /// </summary>
        public void UpdateAllPawnsInteractivity(bool isInteractable = true)
        {
            var allPawns = Board.Pawns;
            foreach (var pawn in allPawns)
            {
                pawn.Interactable = isInteractable;
            }
        }

        /// <summary>
        /// Обновляет интерактивность шашек указанного игрока.
        /// </summary>
        public void UpdatePawnsInteractivity(Player pl, bool isInteractable = true)
        {
            var currentColorPawns = Board.GetPawnsOnBoard(pl.PawnColor);
            foreach (var pawn in currentColorPawns)
            {
                pawn.Interactable = isInteractable;
            }
        }

        /// <summary>
        /// Возвращает противоположный цвет для заданного цвета.
        /// </summary>
        public PawnColor GetOppositeColor(PawnColor color)
        {
            if (color == PawnColor.None) return PawnColor.None;
            return color == PawnColor.White ? PawnColor.Black : PawnColor.White;
        }

        /// <summary>
        /// Возвращает противоположный цвет для заданного игрока.
        /// </summary>
        public PawnColor GetOppositeColor(Player pl)
        {
            if (pl.PawnColor == PawnColor.None) return PawnColor.None;
            return pl.PawnColor == PawnColor.White ? PawnColor.Black : PawnColor.White;
        }
        
        public Player GetOppositePlayer(PawnColor pawnColor)
        {
            return pawnColor == FirstPlayer.PawnColor ? FirstPlayer : SecondPlayer;
        }
        public Player GetOppositePlayer(Player player)
        {
            if (player == FirstPlayer) return SecondPlayer;
            if (player == SecondPlayer) return FirstPlayer;
           
            if (player == null) return null;
            return null;
        }
        
        public void InitPlayerTypes()
        { // todo инициализация игроков, генерация для противника
            FirstPlayer = new Player("игрок1");
            SecondPlayer = new Player("игрок2");
            switch (GameType)
            {
                case GameType.HumanVsHuman:
                    FirstPlayer.Type = PlayerType.Human;
                    SecondPlayer.Type = PlayerType.Human;
                    break;
                case GameType.HumanVsAi:
                    FirstPlayer.Type = PlayerType.Human;
                    SecondPlayer.Type = PlayerType.AI;
                    break;
                case GameType.AiVsAi:
                    FirstPlayer.Type = PlayerType.AI;
                    SecondPlayer.Type = PlayerType.AI;
                    break;
                case GameType.OnWeb:
                    FirstPlayer.Type = PlayerType.Human;
                    SecondPlayer.Type = PlayerType.AI;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(GameType), GameType, null);
            }
        }

        public void WhoseTurnFirst()
        {
            if (FirstPlayer == null || SecondPlayer == null) throw new NullReferenceException("First or second player is null.");
            var player = Random.Range(0, 2) == 0 ? FirstPlayer : SecondPlayer;
            CurrentTurn = player;
        }

        #endregion

        private void InitializeStateMachine()
        {
            AIMove = new AITurn(this, AIController);
            HumanMove = new HumanTurn(this);
            GameOver = new EndGame(this);
            FirstTurn = new FirstTurn(this, AIController);
        }

        public void SwitchPlayer()
        {
            CurrentTurn = GetOppositePlayer(CurrentTurn);
        }
       
        
    }
}