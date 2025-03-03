using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Core
{
    public enum PawnColor
    {
        None,
        Black,
        White
    }

    [RequireComponent(typeof(Rigidbody), typeof(LineRenderer))]
    public class Pawn : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        #region Inspector Variables

        [BoxGroup("Colors Settings")] [SerializeField] private PawnColor pawnColor = PawnColor.Black;

        [BoxGroup("Colors Settings")] [SerializeField] private Material blackColor;

        [BoxGroup("Colors Settings")] [SerializeField] private Material whiteColor;

        [BoxGroup("Colors Settings")] [SerializeField] private MeshRenderer pawnMeshRenderer;

        [BoxGroup("Force Settings")] [SerializeField, MinValue(minValue: 1)] public float minForce = 5f;

        [BoxGroup("Force Settings")] [SerializeField, MinValue(2)] public float maxForce = 350f;

        [BoxGroup("Force Settings")] [SerializeField, SuffixLabel("meters", true)] private float maxDragDistance = 2f;

        [BoxGroup("Line Settings")] [SerializeField, Required] private LineRenderer lineRenderer;

        [BoxGroup("Line Settings")] [SerializeField] private float maxLineLength = 3f;

        [BoxGroup("Line Settings")] [SerializeField, Tooltip("Высота на которой показывается линия удара")] private
            float YOffset = .1f;

        [BoxGroup("Board Settings")] [SerializeField, Tooltip("Высота доски по оси Y")] private float boardHeight = 0.5f;

        #endregion

        #region Actions

        public static Action<Pawn> OnSelect;
        public static Action<Pawn> OnForceApplied;
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

        #region Properties public
        
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

        public bool Interactable { get; set; }
        
        #endregion
        
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
            if (!Interactable) return;
            // если шашка не игрока
            if (!IsPlayersChecker()) return;

            if (!_isSelected) return;

            _dragStartWorldPos = GetBoardIntersectionPoint(eventData.position);
            UpdateLineRenderer();
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            if (!Interactable) return;
            if (!_isSelected) return;
            OnStartDrag?.Invoke(this);
            var lastDirection = ForceDirection;
            ForceDirection = CalculateForce(eventData.position);

            if (ForceDirection == lastDirection) return;
            UpdateLineVisuals();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!Interactable) return;
            if (_isSelected)
            {
                ApplyCalculatedForce();
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
        private void ApplyCalculatedForce()
        {
            ApplyForce(ForceDirection * _currentForce);
        }
        
        public void ApplyForce(Vector3 force)
        {
            _rb.AddForce(force, ForceMode.Impulse);
            ResetSelection();
        }

        #endregion
        

        #region Helper Methods

        private void InitializeComponents()
        {
            _rb = GetComponent<Rigidbody>();
            _mainCamera = Camera.main;

            if (!lineRenderer) lineRenderer = GetComponent<LineRenderer>();


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

        private void UpdateLineRenderer()
        {
            lineRenderer.enabled = true;

            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, transform.position);
        }
        
        private void UpdateLineVisuals()
        {
            var baseLength = _currentForce / maxForce;
            var finalLength = baseLength * maxLineLength;
            var endPosition = transform.position + ForceDirection * finalLength;
            
            if (!lineRenderer.enabled)
            {
                lineRenderer.enabled = true;
            }

            lineRenderer.SetPosition(0, transform.position + new Vector3(0f, YOffset, 0f));
            lineRenderer.SetPosition(1, endPosition + new Vector3(0f, YOffset, 0f));

            // UpdateLineColor();TODO изменение цвета от в зависимости от силы 
        }

        private void ResetSelection()
        {
            lineRenderer.enabled = false;
            _currentForce = 0f;
            // после удара снимаем выбор
            _isSelected = false;
            OnForceApplied?.Invoke(null);
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
        
    }
}