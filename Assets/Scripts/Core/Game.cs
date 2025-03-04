using System;
using AI;
using Services;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Core
{
    public class Game : IDisposable
    {
        public GameType GameType { get; private set; } = GameType.HumanVsAi;
        private PawnColor FirstPlayerColor { get; set; } = PawnColor.None; // цвет первого игрока
        private PawnColor CurrentTurn { get; set; } = PawnColor.None;


        private Board Board { get; }
        private AIController AIController { get; }
        private readonly GameManager gameManager;
        private PawnColor GameResult;

        public Game(GameManager gameManager, Board board, AIController aiController)
        {
            this.gameManager = gameManager;
            Board = board;

            AIController = aiController;
            AIController.Initialize(board);

            Pawn.OnForceApplied += OnForceApplied;
        }

        #region PawnEvents

        private void OnForceApplied(Pawn pawn)
        {
            
            if (EndGame(out GameResult))
            {
                Debug.Log($"Game finished with {GameResult}");
                return;
            }
            SwitchTurn();
        }

        #endregion

        #region GameCircle

        public void StartGame(GameType gameType)
        {
            gameManager.CurrentState = GameState.Gameplay;
            Board.SetupStandardPosition(); // Расстановка шашек

            GameType = gameType;
            // Случайный выбор первого игрока (true - Player1, false - Player2)
            bool isPlayer1First = Random.Range(0, 2) == 0;
            StartFirstTurn(isPlayer1First, gameType);
        }

        private void StartFirstTurn(bool isPlayer1First, GameType gameType)
        {
            Debug.Log("StartFirstTurn");
            if (gameType == GameType.HumanVsAi && !isPlayer1First ||
                gameType == GameType.AiVsAi)
            {
                FirstPlayerColor = Random.Range(0, 2) == 0 ? PawnColor.Black : PawnColor.White;
                CurrentTurn = GetOppositeColor(FirstPlayerColor);
                
                AIController.MakeMove(CurrentTurn);
            }
            else
            {
                // Активируем все шашки для выбора
                var allPawns = Board.GetAllPawnsOnBoard();
                foreach (var pawn in allPawns)
                {
                    pawn.Interactable = true;
                }
            }
        }

        public void SwitchTurn()
        {
            if (CurrentTurn == PawnColor.None)
            {
                Debug.Log("SwitchTurn called without current turn (PawnColor.None)");
                return;
            }
            // Смена хода
            CurrentTurn = CurrentTurn == FirstPlayerColor
                ? GetOppositeColor(FirstPlayerColor)
                : FirstPlayerColor;

            switch (this.GameType)
            {
                case GameType.HumanVsHuman:
                    UpdatePawnsInteractivity();
                    break;

                case GameType.HumanVsAi:
                    // если текущий ход (цвет) равен цвету первого игрока, значит он ходит или 
                    if (CurrentTurn == FirstPlayerColor) UpdatePawnsInteractivity();
                    else AIController.MakeMove(CurrentTurn);
                    break;

                case GameType.AiVsAi:
                    AIController.MakeMove(CurrentTurn);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private bool EndGame(out PawnColor winnerColor)
        {
            var black = Board.GetPawnsOnBoard(PawnColor.Black);
            var white = Board.GetPawnsOnBoard(PawnColor.White);

            if (black.Count == 0 && white.Count == 0)
            {
                winnerColor = PawnColor.None;
                return true;
            }

            if (black.Count == 0)
            {
                winnerColor = PawnColor.White;
                return true;
            }

            if (white.Count == 0)
            {
                winnerColor = PawnColor.Black;
                return true;
            }

            winnerColor = PawnColor.None;
            return false;
        }

        #endregion

        #region Helpers

        private void UpdatePawnsInteractivity()
        {
            var currentColorPawns = Board.GetPawnsOnBoard(CurrentTurn);
            foreach (var pawn in currentColorPawns)
            {
                pawn.Interactable = true;
            }
        }

        private PawnColor GetOppositeColor(PawnColor color)
        {
            return color == PawnColor.White
                ? PawnColor.Black
                : PawnColor.White;
        }

        #endregion

        public void Dispose()
        {
            Pawn.OnForceApplied -= OnForceApplied;
        }
    }
}