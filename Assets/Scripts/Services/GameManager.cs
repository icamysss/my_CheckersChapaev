using System;
using Game;
using UnityEngine;

namespace Services
{
    public class GameManager : MonoBehaviour, IGameManager
    {
        [SerializeField] private Board boardPrefab;
        [SerializeField] private AIController aiControllerPrefab;


        public void StartGame(GameType gameType)
        {
            throw new NotImplementedException();
        }

        public void Initialize()
        {
           Debug.Log("Initializing Game Manager");
           isInitialized = true;
        }

        public void Shutdown()
        {
            throw new NotImplementedException();
        }

        public bool isInitialized { get; private set; }
    }

   
}