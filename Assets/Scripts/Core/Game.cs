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
        private PawnColor FirstPlayerColor { get; set; } = PawnColor.None; // Цвет первого игрока
        private PawnColor CurrentTurn { get; set; } = PawnColor.None; // Текущий ход (цвет игрока)

        private Board Board { get; }
        private AIController AIController { get; }
        private readonly GameManager gameManager;
        private PawnColor GameResult; // Результат игры (победитель или ничья)

        private bool isPlayer1First; // Флаг, указывающий, начинает ли первый игрок (true - Player1, false - Player2)
        
        private float delayStartTime; // Время начала задержки
        private bool isDelaying = false; // Флаг активной задержки
        private const float MoveSettleDelay = 1f; // Продолжительность задержки в секундах

        /// <summary>
        /// Конструктор игры, инициализирует зависимости и подписывается на события шашек.
        /// </summary>
        public Game(GameManager gameManager, Board board, AIController aiController)
        {
            this.gameManager = gameManager;
            Board = board;
            AIController = aiController;
            AIController.Initialize(board);

            Pawn.OnForceApplied += OnForceApplied;
            Pawn.OnSelect += OnSelect;
        }

        #region GameEvents
    
        public Action OnGameStart;
        public Action<PawnColor> OnGameEnd;
        public Action OnEndMove;
        
        #endregion
        
        #region PawnEvents

        /// <summary>
        /// Обработчик выбора шашки. Устанавливает начальный ход и цвет первого игрока при первом выборе.
        /// </summary>
        private void OnSelect(Pawn pawn)
        {
            if (CurrentTurn == PawnColor.None)
            {
                CurrentTurn = pawn.PawnColor;
                FirstPlayerColor = isPlayer1First ? pawn.PawnColor : GetOppositeColor(pawn.PawnColor);
            }
        }

        /// <summary>
        /// Обработчик применения силы к шашке. Запускает задержку перед проверкой окончания игры.
        /// </summary>
        private void OnForceApplied(Pawn pawn)
        {
            // Отключаем взаимодействие всех шашек
            var allPawns = Board.GetAllPawnsOnBoard();
            foreach (var p in allPawns)
            {
                p.Interactable = false;
            }

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
            isPlayer1First = Random.Range(0, 2) == 0;
            StartFirstTurn(isPlayer1First, gameType);
            
            OnGameStart?.Invoke();
        }

        /// <summary>
        /// Начинает первый ход в зависимости от типа игры и того, кто ходит первым.
        /// </summary>
        private void StartFirstTurn(bool isPlayer1First, GameType gameType)
        {
            Debug.Log("Начало первого хода");
            if (gameType == GameType.HumanVsAi && !isPlayer1First || gameType == GameType.AiVsAi)
            {
                FirstPlayerColor = Random.Range(0, 2) == 0 ? PawnColor.Black : PawnColor.White;
                CurrentTurn = GetOppositeColor(FirstPlayerColor);
                AIController.MakeMove(CurrentTurn);
            }
            else
            {
                var allPawns = Board.GetAllPawnsOnBoard();
                foreach (var pawn in allPawns)
                {
                    pawn.Interactable = true;
                }
            }
        }

        /// <summary>
        /// Переключает ход между игроками в зависимости от типа игры.
        /// </summary>
        private void SwitchTurn()
        {
            if (CurrentTurn == PawnColor.None)
            {
                Debug.Log("Переключение хода вызвано без текущего хода (PawnColor.None)");
                return;
            }

            CurrentTurn = CurrentTurn == FirstPlayerColor
                ? GetOppositeColor(FirstPlayerColor)
                : FirstPlayerColor;

            switch (GameType)
            {
                case GameType.HumanVsHuman:
                    UpdatePawnsInteractivity();
                    break;

                case GameType.HumanVsAi:
                    if (CurrentTurn == FirstPlayerColor)
                    {
                        UpdatePawnsInteractivity();
                    }
                    else
                    {
                        AIController.MakeMove(CurrentTurn);
                    }
                    break;

                case GameType.AiVsAi:
                    AIController.MakeMove(CurrentTurn);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            OnEndMove?.Invoke();
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
        private void UpdatePawnsInteractivity()
        {
            var currentColorPawns = Board.GetPawnsOnBoard(CurrentTurn);
            foreach (var pawn in currentColorPawns)
            {
                pawn.Interactable = true;
            }
        }

        /// <summary>
        /// Возвращает противоположный цвет для заданного цвета.
        /// </summary>
        private PawnColor GetOppositeColor(PawnColor color)
        {
            return color == PawnColor.White ? PawnColor.Black : PawnColor.White;
        }

        #endregion

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
        /// Очищает подписку на события при завершении игры.
        /// </summary>
        public void Dispose()
        {
            Pawn.OnForceApplied -= OnForceApplied;
            Pawn.OnSelect -= OnSelect;
        }
    }
}