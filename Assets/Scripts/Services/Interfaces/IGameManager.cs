using System;
using Core;

namespace Services
{
    public interface IGameManager : IService
    {
        public event Action<GameState> OnGameStateChanged;
        public GameState CurrentState { get; set; }
        public Game CurrentGame { get; }
        
    }

    
}