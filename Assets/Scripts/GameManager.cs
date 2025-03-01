using System;
using DefaultNamespace;
using UnityEngine;

public enum GameType { HvsH, HvsAI, AIvsAI} 

public class GameManager: Singleton<GameManager>
{
    public static GameManager Instance;

    [SerializeField] private Board boardPrefab;
    [SerializeField] private AIController aiControllerPrefab;
    [SerializeField] private UIManager uiManagerPrefab;


    public void StartGame(GameType gameType)
    {
        throw new NotImplementedException();
    }

   
}
