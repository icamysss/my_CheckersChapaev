using UnityEngine;

public class GameManager: Singleton<GameManager>
{
    [SerializeField] private CameraController cameraController;
    
    public Checker SelectedChecker { get; private set; }

    private void Awake()
    {
        InitializeComponents();
    }
    
    private void InitializeComponents()
    {
        if (cameraController == null)
        {
            cameraController = FindFirstObjectByType<CameraController>();
            if (cameraController == null) throw new UnityException("No cameraController found");
        }
    }
    
    public void SelectChecker(Checker checker)
    {
        SelectedChecker = checker;
        cameraController.SetTarget(checker);
    }

    public void DeselectChecker()
    {
        SelectedChecker = null;
        cameraController.SetTarget(null);
    }
    
    
}