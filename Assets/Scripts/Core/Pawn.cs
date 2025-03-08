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
        
        [BoxGroup("Colors Settings")] [SerializeField, Tooltip("Объект который отображает выбор шашки")] private GameObject ringSelect;
        
        

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
        private bool isOnBoard;

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
            _audioService.PawnAudio.PlayStrikeSound();
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
            ringSelect.SetActive(false);
            OnForceApplied?.Invoke(null);
        }

        public void Select()
        {
            _isSelected = true;
            ringSelect.SetActive(true);
            OnSelect?.Invoke(this);
        }
        
        #endregion
        
        #region Unity Lifecycle

        private void OnEnable()
        {
            // если выбрали шашку, сообщаем остальным шашкам, что выбрана эта
            OnSelect += OnSelected;
        }

        private void OnDisable()
        {
            OnSelect -= OnSelected;
        }

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
            if (!collision.gameObject.CompareTag("Board")) _audioService.PawnAudio.PlayCollideSound();
        }

        private void OnCollisionStay(Collision other)
        {
            if (!other.gameObject.CompareTag("Board")) return;
            isOnBoard = true;
        }

        private void OnCollisionExit(Collision other)
        {
            if (!other.gameObject.CompareTag("Board")) return;
            isOnBoard = false;
        }

        private void Update()
        {
            float minSpeed = 1f;
            float maxSpeed = 100f;
            // Проверяем, если шашка на доске и движется быстрее минимальной скорости
            if (isOnBoard && _rb.linearVelocity.magnitude > minSpeed)
            {
                // Включаем звук, если он ещё не играет
                if (!_audioSource.isPlaying)
                {
                    _audioSource.Play();
                }

                // Вычисляем громкость на основе скорости
                float speed = _rb.linearVelocity.magnitude;
                float targetVolume = Mathf.Clamp01((speed - minSpeed) / (maxSpeed - minSpeed));

                // Плавно изменяем громкость
                _audioSource.volume = Mathf.Lerp(_audioSource.volume, targetVolume, Time.deltaTime * 5);
            }
            else
            {
                // Если шашка остановилась или оторвалась от доски
                if (_audioSource.isPlaying)
                {
                    // Плавно уменьшаем громкость перед остановкой
                    if (_audioSource.volume > 0.01f)
                    {
                        _audioSource.volume = Mathf.Lerp(_audioSource.volume, 0f, Time.deltaTime * 5);
                    }
                    else
                    {
                        _audioSource.Stop(); // Останавливаем звук, когда громкость почти нулевая
                        _audioSource.volume = 0f;
                    }
                }
            }
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
               Select();
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

        private void OnSelected(Pawn selectedPawn)
        {
            if (selectedPawn != this) ResetSelection();
        }
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
            
            if (ringSelect == null) throw new MissingComponentException("Ring Select not found");
            ringSelect.SetActive(false);
           
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