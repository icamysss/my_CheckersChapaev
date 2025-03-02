using System;

namespace Core
{
    [Serializable]
    public class Game 
    {
        public GameType GameType { get; private set; }
        public PawnColor HumanColor { get; private set; }
        public PawnColor CurrentTurn { get; private set; }
        public Board Board { get; set; }
        public AIController AIController { get; set; }


        public void StartGame()
        {
            // Board.Initialize(); // Расстановка шашек
            CurrentTurn = PawnColor.White;
            if (GameType == GameType.HumanVsAi && HumanColor != CurrentTurn) 
            {
                AIController.MakeMove(); // ИИ ходит первым
            }
        }

        public void PauseGame()
        {
            throw new NotImplementedException();
        }

        public void MakeMove()
        {
            // Логика хода, проверка на выбивание шашек
            // Переключение CurrentTurn
            // if (CheckWinCondition()) 
            // {
            //     GameManager.Instance.SetGameState(GameState.GameOver);
            // }
        }

        public void EndGame()
        {
            throw new NotImplementedException();
        }
    }
}