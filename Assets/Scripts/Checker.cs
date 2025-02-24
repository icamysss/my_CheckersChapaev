using System;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening; 
using Sirenix.OdinInspector;

[RequireComponent(typeof(Rigidbody), typeof(LineRenderer))]
public class Checker : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    #region Inspector Variables
    [BoxGroup("Force Settings", true)]
    [SerializeField, MinValue(minValue: 1)] private float minForce = 5f;
    [BoxGroup("Force Settings")]
    [SerializeField, MinValue(1)] private float maxForce = 20f;
    [BoxGroup("Force Settings")]
    [SerializeField, SuffixLabel("meters", true)] private float maxDragDistance = 2f;
    [BoxGroup("Visual Settings", true)]
    [SerializeField, Required] private LineRenderer lineRenderer;
    [BoxGroup("Visual Settings")]
    [SerializeField, Range(0.01f, 2f)] private float lineLengthMultiplier = 0.3f;
    [BoxGroup("Visual Settings")]
    [SerializeField] private float maxLineLength = 3f;
    [BoxGroup("Visual Settings")]
    [SerializeField] private Ease lineAnimationEase = Ease.OutQuad;

    [BoxGroup("Board Settings", true)]
    [SerializeField, Tooltip("Высота доски по оси Y")] 
    private float boardHeight = 0.5f;
    [BoxGroup("Debug")]
    [SerializeField, ReadOnly] private float lastAppliedForce;
    #endregion
   
    [HideInInspector]
   
    
    
    public static Action<Checker> OnSelect;
    public static Action<Checker> OnDeselect;
    public static Action<Checker> OnStartDrag;
    
    
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
        var LastDirection = ForceDirection;
        ForceDirection = CalculateForce(eventData.position);
        
        if (ForceDirection == LastDirection) return;
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
        float baseLength = _currentForce * lineLengthMultiplier;
        float finalLength = Mathf.Min(baseLength, maxLineLength);
        Vector3 endPosition = transform.position + ForceDirection * finalLength;

        // Анимация линии с DOTween
        if (!lineRenderer.enabled)
        {
            lineRenderer.enabled = true;
            AnimateLineAppearance();
        }

        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, endPosition);

        UpdateLineColor();
    }

    private void AnimateLineAppearance()
    {
        _lineAnimationTween?.Kill();
        
        // Анимация ширины линии
        lineRenderer.widthMultiplier = 0f;
        _lineAnimationTween = DOTween.To(
            () => lineRenderer.widthMultiplier,
            x => lineRenderer.widthMultiplier = x,
            0.1f, // Конечная ширина
            0.3f   // Длительность анимации
        ).SetEase(lineAnimationEase);
    }

    private void UpdateLineColor()
    {
        float t = _currentForce / maxForce;
        Color gradientColor = Color.Lerp(Color.green, Color.red, t);
    
        // Для материалов с поддержкой Vertex Colors
        lineRenderer.colorGradient = new Gradient()
        {
            alphaKeys = new[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 1) },
            colorKeys = new[] { new GradientColorKey(gradientColor, 0), new GradientColorKey(gradientColor, 1) }
        };
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
    }

    private void SetupLineRenderer()
    {
        lineRenderer.positionCount = 2;
        lineRenderer.enabled = false;
    }

    private Vector3 GetBoardIntersectionPoint(Vector2 screenPos)
    {
        Ray ray = _mainCamera.ScreenPointToRay(screenPos);
        Plane boardPlane = new Plane(Vector3.up, new Vector3(0, boardHeight, 0));
        
        return boardPlane.Raycast(ray, out float distance) 
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
        lastAppliedForce = 0f;
        // после удара снимаем выбор
        _isSelected = false;
        OnDeselect?.Invoke(null);
    }

    private void ApplyForce(Vector3 force)
    {
        _rb.AddForce(force, ForceMode.Impulse);
        lastAppliedForce = force.magnitude;
        
        // GameManager.Instance.SwitchTurn();
    }

    [ShowInInspector, BoxGroup("Debug"), ReadOnly]
    private bool IsPlayersChecker()
    {
        // todo Заглушка - реализуйте свою логику
        return true;
    }
    #endregion
}