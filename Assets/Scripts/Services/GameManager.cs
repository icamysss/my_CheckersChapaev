using System;
using UnityEngine;

namespace Services
{
    public enum GameType
    {
        HvsH,   // человек / человек
        HvsAI,  // человек / компьютер 
        AIvsAI  // компьютер / компьютер
    }

    public class GameManager : Singleton<GameManager>
    {
        [SerializeField] private Board boardPrefab;
        [SerializeField] private AIController aiControllerPrefab;


        public void StartGame(GameType gameType)
        {
            throw new NotImplementedException();
        }
    }
}