using AI;
using UnityEngine;

namespace Core
{

    public class Player
    {
        public string Name;
        public PawnColor PawnColor;
        public PlayerType Type;
        public AISettings AISettings;
        
        public Player(string name, PawnColor pawnColor, PlayerType playerType)
        {
            Name = name;
            PawnColor = pawnColor;
            Type = playerType;
            AISettings = new AISettings();
        }
        
        public Player(string name)
        {
            Name = name;
            PawnColor = PawnColor.None;
            Type = PlayerType.Human;
            AISettings = new AISettings();
        }
        
        
    }
    
    public enum PlayerType
    {
        Human,
        AI
    }
}