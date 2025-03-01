using System;
using UnityEngine;

public enum PlayerType { Human, AI }

public class GameManager: Singleton<GameManager>
{
    public static GameManager Instance;

    [SerializeField] private Board boardPrefab;
    [SerializeField] private AIController aiController;
    

   

   
}
