using System;
using UnityEngine;

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
            Debug.Log($"Enter {this.GetType().Name}");
        }

        public virtual void Exit()
        {
            Debug.Log($"Exit {this.GetType().Name}");
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