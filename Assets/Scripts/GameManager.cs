using UnityEngine;

public class GameManager: Singleton<GameManager>
{
    
    public Checker SelectedChecker { get; set; }

    private void Awake()
    {
        InitializeComponents();
    }
    
    private void InitializeComponents()
    {
    }
}