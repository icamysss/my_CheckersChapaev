using System;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening; 
using Sirenix.OdinInspector;

public enum PawnColor
{
    Black,
    White
}

[RequireComponent(typeof(Rigidbody), typeof(LineRenderer))]
public class Pawn : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    #region Inspector Variables
    [BoxGroup("Colors Settings")]
    [SerializeField] 
    private PawnColor pawnColor = PawnColor.Black;
    
    [BoxGroup("Colors Settings")]
    [SerializeField] 
    private Material blackColor;
    
    [BoxGroup("Colors Settings")]
    [SerializeField] 
    private Material whiteColor ;
    
    [BoxGroup("Colors Settings")]
    [SerializeField] 
    private MeshRenderer pawnMeshRenderer;
    
    [BoxGroup("Force Settings")]
    [SerializeField, MinValue(minValue: 1)] 
    public float minForce = 5f;
   
    [BoxGroup("Force Settings")]
    [SerializeField, MinValue(2)]
    public float maxForce = 350f;
   
    [BoxGroup("Force Settings")]
    [SerializeField, SuffixLabel("meters", true)] 
    private float maxDragDistance = 2f;
   
    [BoxGroup("Line Settings")]
    [SerializeField, Required] 
    private LineRenderer lineRenderer;
   
    [BoxGroup("Line Settings")]
    [SerializeField] 
    private float maxLineLength = 3f;
    [BoxGroup("Line Settings")]
    [SerializeField, Tooltip("Высота на которой показывается линия удара")] 
    private float YOffset = .1f;
    
    [BoxGroup("Board Settings")]
    [SerializeField, Tooltip("Высота доски по оси Y")] 
    private float boardHeight = 0.5f;
    
    
    #endregion
    
    #region Actions
    
    public static Action<Pawn> OnSelect;
    public static Action<Pawn> OnDeselect;
    public static Action<Pawn> OnStartDrag;
    
    #endregion
    
    #region Private Variables
    private Rigidbody _rb;
    private Camera _mainCamera;
    private Vector3 _dragStartWorldPos;
    private bool _isSelected;
    private float _currentForce;
    private Tween _lineAnimationTween;
    private CameraController cameraController;
    #endregion

    private Vector3 ForceDirection { get; set; }
    public PawnColor PawnColor
    {
        get => pawnColor;
        set
        {
            pawnColor = value;
            UpdateMeshRenderer();
        }
    }

    #region Unity Lifecycle
    private void Awake()
    {
        InitializeComponents();
        SetupLineRenderer();
    }

    private void OnDestroy()
    {
        // Очищаем твины при уничтожении объекта
        _lineAnimationTween?.Kill();
    }

    private void OnValidate()
    {
       if (lineRenderer == null) lineRenderer = GetComponent<LineRenderer>();
       if (minForce > maxForce) minForce = maxForce;
       
       UpdateMeshRenderer();
    }

    #endregion

    #region Input Handlers
    public void OnPointerDown(PointerEventData eventData)
    {
        // если шашка не игрока
        if (!IsPlayersChecker()) return;

        if (!_isSelected) return;
        
        _dragStartWorldPos = GetBoardIntersectionPoint(eventData.position);
        StartSelection();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_isSelected) return;
        OnStartDrag?.Invoke(this);
        var lastDirection = ForceDirection;
        ForceDirection = CalculateForce(eventData.position);
        
        if (ForceDirection == lastDirection) return;
        UpdateLineVisuals();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (_isSelected)
        {
            ApplyCalculatedForce();
            ResetSelection();
        }
        else // если не выбрана
        {
            _isSelected = true;
            OnSelect?.Invoke(this);
        }
    }
    #endregion

    #region Force Logic
    private Vector3 CalculateForce(Vector2 screenPosition)
    {
        var currentWorldPos = GetBoardIntersectionPoint(screenPosition);
        var dragVector = currentWorldPos - _dragStartWorldPos;
        
        _currentForce = Mathf.Lerp(minForce, maxForce, 
            Mathf.Clamp01(dragVector.magnitude / maxDragDistance));

        ForceDirection = -dragVector.normalized;
        return ForceDirection;
    }
    #endregion

    #region Visual Effects
    private void UpdateLineVisuals()
    {
        var baseLength = _currentForce / maxForce;
        float finalLength = baseLength * maxLineLength;
        Vector3 endPosition = transform.position + ForceDirection * finalLength;

        // Анимация линии с DOTween
        if (!lineRenderer.enabled)
        {
            lineRenderer.enabled = true;
        }

        lineRenderer.SetPosition(0, transform.position + new Vector3(0f, YOffset, 0f));
        lineRenderer.SetPosition(1, endPosition + new Vector3(0f, YOffset, 0f));

       // UpdateLineColor();TODO изменение цвета от в зависимости от силы 
    }
    #endregion

    #region Game Logic
    private void ApplyCalculatedForce()
    {
        ApplyForce(ForceDirection * _currentForce);
    }
    #endregion

    #region Helper Methods
    private void InitializeComponents()
    {
        _rb = GetComponent<Rigidbody>();
        _mainCamera = Camera.main;
        
        if (!lineRenderer)   lineRenderer = GetComponent<LineRenderer>();
        
        
        cameraController = FindFirstObjectByType<CameraController>();
        if (!cameraController) throw new MissingComponentException("Missing CameraController");
        
        if (pawnMeshRenderer == null) pawnMeshRenderer = GetComponentInChildren<MeshRenderer>();
        if (pawnMeshRenderer == null) throw new MissingComponentException("Pawn Mesh Renderer not found");
    }

    private void SetupLineRenderer()
    {
        lineRenderer.positionCount = 2;
        lineRenderer.enabled = false;
    }

    private Vector3 GetBoardIntersectionPoint(Vector2 screenPos)
    {
        var ray = _mainCamera.ScreenPointToRay(screenPos);
        var boardPlane = new Plane(Vector3.up, new Vector3(0, boardHeight, 0));
        
        return boardPlane.Raycast(ray, out var distance) 
            ? ray.GetPoint(distance) 
            : Vector3.zero;
    }

    private void StartSelection()
    {
        _isSelected = true;
        lineRenderer.enabled = true;
        
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, transform.position);
    }

    private void ResetSelection()
    {
        lineRenderer.enabled = false;
        _currentForce = 0f;
        // после удара снимаем выбор
        _isSelected = false;
        OnDeselect?.Invoke(null);
    }

    public void ApplyForce(Vector3 force)
    {
        _rb.AddForce(force, ForceMode.Impulse);
        
        // GameManager.Instance.SwitchTurn();
    }

    [ShowInInspector, BoxGroup("Debug"), ReadOnly]
    private bool IsPlayersChecker()
    {
        // todo Заглушка - реализуйте свою логику
        return true;
    }

    private void UpdateMeshRenderer()
    {
        if (pawnMeshRenderer == null) pawnMeshRenderer = GetComponentInChildren<MeshRenderer>();
        if (pawnMeshRenderer == null) throw new MissingComponentException("Pawn Mesh Renderer not found");
        
        pawnMeshRenderer.material = pawnColor == PawnColor.Black ? blackColor : whiteColor;
    }
    #endregion
    
    #region Public Methods

    public void SetColor(PawnColor color)
    {
        pawnColor = color;
        UpdateMeshRenderer();
    }
    
    #endregion
}