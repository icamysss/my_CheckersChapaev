using System;
using Core;

namespace Services
{
    public interface IGameManager : IService
    {
        public event Action<ApplicationState> OnGameStateChanged;
        public ApplicationState CurrentState { get; set; }
        public Game CurrentGame { get; }
        
    }

    
}