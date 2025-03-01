using System;
using UnityEngine;

public class GameManager: Singleton<GameManager>
{
    public AIController aiController;
    public Pawn SelectedChecker { get; set; }

    private void Awake()
    {
        InitializeComponents();
    }
    
    private void InitializeComponents()
    {
        aiController = FindFirstObjectByType<AIController>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) aiController.MakeMove();
    }
}