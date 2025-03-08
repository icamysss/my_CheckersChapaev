using System;

namespace Core
{
    [Serializable]
    public class Player
    {
        public string Name;
        public PawnColor PawnColor;
        public PlayerType Type;

      
        
        public Player(string name, PawnColor pawnColor, PlayerType playerType)
        {
            Name = name;
            PawnColor = pawnColor;
            Type = playerType;
        }
    }
    
    public enum PlayerType
    {
        Human,
        AI
    }
}