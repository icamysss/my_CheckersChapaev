using System;
using AI;
using Services;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Core
{
    public class Game : IDisposable
    {
        private GameType GameType { get; set; } = GameType.HumanVsAi;
        private PawnColor GameResult; // Результат игры (победитель или ничья)

        public Player CurrentTurn { get; private set; }  // Текущий ход ( игрока)
        public Player FirstTurn   { get; private set; }  
        public Player FirstPlayer { get; private set; }
        public Player SecondPlayer{ get; private set; }
     
        public Board Board { get; }
        private AIController AIController { get; }
        
        private float delayStartTime; // Время начала задержки
        private bool isDelaying = false; // Флаг активной задержки
        private const float MoveSettleDelay = 1f; // Продолжительность задержки в секундах
        private bool SelectingColor = false; // Флаг выбора цвета на первом ходу
        private readonly GameManager gameManager;
        
        
        /// <summary>
        /// Конструктор игры, инициализирует зависимости и подписывается на события шашек.
        /// </summary>
        public Game(GameManager gameManager, Board board)
        {
            this.gameManager = gameManager;
            Board = board;
            
            this.AIController = new AIController();
            this.AIController.Initialize(board);
            Pawn.OnEndAiming += OnForceApplied;
            Pawn.OnSelect += OnSelect;
            
            FirstPlayer = new Player("player1", PawnColor.None, PlayerType.AI);
            SecondPlayer = new Player("player2", PawnColor.None, PlayerType.AI);
        }

        #region GameEvents
    
        public Action OnGameStart;
        public Action<PawnColor> OnGameEnd;
        public Action OnEndTurn;
        public Action OnStartTurn;
        
        #endregion
        
        #region PawnEvents

        /// <summary>
        /// Обработчик выбора шашки. Устанавливает начальный ход и цвет первого игрока при первом выборе.
        /// </summary>
        private void OnSelect(Pawn pawn)
        {
            // если не определен цвет игроков
            if (SelectingColor)
            {
                FirstPlayer.PawnColor = CurrentTurn == FirstPlayer ? pawn.PawnColor : GetOppositeColor(pawn.PawnColor);
                SecondPlayer.PawnColor = GetOppositeColor(FirstPlayer);
            }
        }

        /// <summary>
        /// Обработчик применения силы к шашке. Запускает задержку перед проверкой окончания игры.
        /// </summary>
        private void OnForceApplied(Pawn pawn)
        {
            // Отключаем взаимодействие всех шашек
           UpdateAllPawnsInteractivity(false);
            OnEndTurn?.Invoke();
            // Запускаем задержку
            isDelaying = true;
            delayStartTime = Time.time;
        }

        #endregion

        #region GameCircle

        /// <summary>
        /// Запускает игру: настраивает доску и определяет, кто ходит первым.
        /// </summary>
        public void StartGame(GameType gameType)
        {
            gameManager.CurrentState = GameState.Gameplay;
            Board.SetupStandardPosition();

            GameType = gameType;
            switch (gameType)
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
                default:
                    throw new ArgumentOutOfRangeException(nameof(gameType), gameType, null);
            }
            
            FirstTurn = Random.Range(0, 2) == 0 ? FirstPlayer : SecondPlayer;
            CurrentTurn = FirstTurn;
            StartFirstTurn(FirstTurn, gameType);
            
            OnGameStart?.Invoke();
        }

        /// <summary>
        /// Метод обновления, вызываемый каждый кадр для обработки задержки.
        /// </summary>
        public void GameUpdate()
        {
            if (isDelaying && Time.time >= delayStartTime + MoveSettleDelay)
            {
                isDelaying = false;
                if (EndGame(out GameResult))
                {
                    Debug.Log($"Игра завершилась с результатом: {GameResult}");
                    return;
                }
                SwitchTurn();
            }
        }
        
        /// <summary>
        /// Начинает первый ход в зависимости от типа игры и того, кто ходит первым.
        /// </summary>
        private void StartFirstTurn(Player firsTurn, GameType gameType)
        {
            Debug.Log("Начало первого хода");
            if (firsTurn.Type == PlayerType.AI)
            {
                // ИИ Выбирает цвет
                firsTurn.PawnColor = Random.Range(0, 2) == 0 ? PawnColor.Black : PawnColor.White;
                if (firsTurn == FirstPlayer) SecondPlayer.PawnColor = GetOppositeColor(firsTurn);
                else FirstPlayer.PawnColor = GetOppositeColor(SecondPlayer);
                
                AIController.MakeMove(firsTurn);
            }
            else
            {  // Игрок выбирает цвет
                
                var allPawns = Board.GetAllPawnsOnBoard();
                foreach (var pawn in allPawns)
                {
                    pawn.Interactable = true;
                }
                SelectingColor = true;
            }
        }

        /// <summary>
        /// Переключает ход между игроками в зависимости от типа игры.
        /// </summary>
        private void SwitchTurn()
        {
            if (CurrentTurn == null)
            {
                Debug.Log("Current turn is null");
                return;
            }

            // смена игрока 
            CurrentTurn = CurrentTurn == FirstPlayer
                ? SecondPlayer
                : FirstPlayer;

            // Начало хода
            OnStartTurn?.Invoke();
            
            if (CurrentTurn.Type == PlayerType.AI)
            {
                AIController.MakeMove(CurrentTurn);
            }
            else
            {
                UpdatePawnsInteractivity(CurrentTurn, true);
            }
            
        }

        /// <summary>
        /// Проверяет, закончилась ли игра, и определяет победителя.
        /// </summary>
        private bool EndGame(out PawnColor winnerColor)
        {
            var black = Board.GetPawnsOnBoard(PawnColor.Black);
            var white = Board.GetPawnsOnBoard(PawnColor.White);

            if (black.Count == 0 && white.Count == 0)
            {
                winnerColor = PawnColor.None; // Ничья
                OnGameEnd?.Invoke(PawnColor.None);
                return true;
            }

            if (black.Count == 0)
            {
                winnerColor = PawnColor.White; // Белые победили
                OnGameEnd?.Invoke(PawnColor.White);
                return true;
            }

            if (white.Count == 0)
            {
                winnerColor = PawnColor.Black; // Черные победили
                OnGameEnd?.Invoke(PawnColor.Black);
                return true;
            }

            winnerColor = PawnColor.None; // Игра продолжается
            return false;
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Обновляет интерактивность шашек текущего игрока.
        /// </summary>
        private void UpdateAllPawnsInteractivity(bool isInteractable = true)
        {
            var allPawns = Board.GetAllPawnsOnBoard();
            foreach (var pawn in allPawns)
            {
                pawn.Interactable = isInteractable;
            }
        }
        
        /// <summary>
        /// Обновляет интерактивность шашек текущего игрока.
        /// </summary>
        private void UpdatePawnsInteractivity(Player pl, bool isInteractable = true)
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
        private PawnColor GetOppositeColor(PawnColor color)
        {
            if (color == PawnColor.None) return PawnColor.None;
            return color == PawnColor.White ? PawnColor.Black : PawnColor.White;
        }
        
        /// <summary>
        /// Возвращает противоположный цвет для заданного игрока.
        /// </summary>
        private PawnColor GetOppositeColor(Player pl)
        {
            if (pl.PawnColor == PawnColor.None) return PawnColor.None;
            return pl.PawnColor == PawnColor.White ? PawnColor.Black : PawnColor.White;
        }

        #endregion
        
        /// <summary>
        /// Очищает подписку на события при завершении игры.
        /// </summary>
        public void Dispose()
        {
            Pawn.OnEndAiming -= OnForceApplied;
            Pawn.OnSelect -= OnSelect;
        }
    }
}