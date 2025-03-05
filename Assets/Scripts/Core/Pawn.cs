using System;
using DG.Tweening;
using Services;
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
            float yOffset = .1f;

        [BoxGroup("Board Settings")] [SerializeField, Tooltip("Высота доски по оси Y")] private float boardHeight = 0.5f;

        #endregion

        #region Actions

        public static Action<Pawn> OnSelect;
        public static Action<Pawn> OnForceApplied;
        public static Action<Pawn> OnStartDrag;

        #endregion

        #region Private Variables

        private Rigidbody _rb;
        private AudioSource _audioSource;
        private Vector3 _dragStartWorldPos;
        private bool _isSelected;
        private float _currentForce;
        private Tween _lineAnimationTween;
        private ICameraController _cameraController;
        private IAudioService _audioService;

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
        
        #region Public Methods
        
        public void ApplyForce(Vector3 force)
        {
            _rb.AddForce(force, ForceMode.Impulse);
            ResetSelection();
        }
        public void UpdateLineVisuals(float force, Vector3 forceDirection)
        {
            var baseLength = force / maxForce;
            var finalLength = baseLength * maxLineLength;
            var endPosition = transform.position + forceDirection * finalLength;
            
            if (!lineRenderer.enabled)
            {
                lineRenderer.enabled = true;
            }

            lineRenderer.SetPosition(0, transform.position + new Vector3(0f, yOffset, 0f));
            lineRenderer.SetPosition(1, endPosition + new Vector3(0f, yOffset, 0f));

            // UpdateLineColor();TODO изменение цвета от в зависимости от силы 
        }

        public void ResetSelection()
        {
            SwitchLineRenderer();
            _currentForce = 0f;
            // после удара снимаем выбор
            _isSelected = false;
            OnForceApplied?.Invoke(null);
        }

        public void Select()
        {
            _isSelected = true;
            OnSelect?.Invoke(this);
        }
        
        #endregion
        
        #region Unity Lifecycle

        private void Awake()
        {
            InitializeComponents();
            SwitchLineRenderer();
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

        private void OnCollisionEnter(Collision collision)
        {
            if (!collision.gameObject.CompareTag("Board")) _audioService.PawnAudio.PlayStrikeSound(_audioSource);
        }
        
        private void OnCollisionStay(Collision collision)
        {
           // if (collision.gameObject.CompareTag("Board") && _rb.linearVelocity.magnitude > 0.2f )
               // _audioService.PawnAudio.StartMovementLoop(_audioSource);
        }

        private void OnCollisionExit(Collision collision)
        {
            if (collision.gameObject.CompareTag("Board") && _rb.linearVelocity.magnitude > 0 )
                _audioService.PawnAudio.StopMovementLoop(_audioSource);
        }

        #endregion

        #region Input Handlers

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!Interactable) return;

            if (!_isSelected) return;

            _dragStartWorldPos = GetBoardIntersectionPoint(eventData.position);
            SwitchLineRenderer(true);
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            if (!Interactable) return;
            if (!_isSelected) return;
            OnStartDrag?.Invoke(this);
            var lastDirection = ForceDirection;
            ForceDirection = CalculateForce(eventData.position);

            if (ForceDirection == lastDirection) return;
            UpdateLineVisuals(_currentForce, ForceDirection);
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
        
        #endregion
        
        #region Helper Methods

        private void InitializeComponents()
        {
            _rb = GetComponent<Rigidbody>();
            
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null) throw new MissingComponentException("Audio Source is null");
            
            if (!lineRenderer) lineRenderer = GetComponent<LineRenderer>();
            
            if (pawnMeshRenderer == null) pawnMeshRenderer = GetComponentInChildren<MeshRenderer>();
            if (pawnMeshRenderer == null) throw new MissingComponentException("Pawn Mesh Renderer not found");

            _cameraController = ServiceLocator.Get<ICameraController>();
            _audioService = ServiceLocator.Get<IAudioService>();
           
        }

        private void SwitchLineRenderer(bool enable = false)
        {
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, transform.position);
            
            lineRenderer.enabled = enable;
        }

        private Vector3 GetBoardIntersectionPoint(Vector2 screenPos)
        {
            var ray = _cameraController.MainCamera.ScreenPointToRay(screenPos);
            var boardPlane = new Plane(Vector3.up, new Vector3(0, boardHeight, 0));

            return boardPlane.Raycast(ray, out var distance)
                ? ray.GetPoint(distance)
                : Vector3.zero;
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