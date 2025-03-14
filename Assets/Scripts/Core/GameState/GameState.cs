using System;

namespace Core.GameState
{
    public abstract class GameState
    {
        private protected readonly Game ThisGame;

        protected GameState(Game game)
        {
            ThisGame = game;
        }

        public virtual void Enter()
        {
            // прервать все асинхронные методы связанныет с прошлым ходом
            ThisGame.CancelAllAsyncOnLastTurn();
            //проверить условия конца игры, закончить игру
            if (ThisGame.IsGameOver()) ThisGame.CurrentState = ThisGame.GameOver;
        }

        public virtual void Exit()
        {
        }


        /// <summary>
        /// Следущее состояние, Проверяет на условия победы
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public virtual void Next()
        {
            ThisGame.UpdateAllPawnsInteractivity(false);
        }
    }
}