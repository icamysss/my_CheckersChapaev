using UnityEngine;
using DG.Tweening;

public class CameraController : MonoBehaviour
{
    [Header("Base Settings")]
    [SerializeField] private float transitionDuration = 0.5f;
    [SerializeField] private Ease transitionEase = Ease.OutQuad;
    
    [Header("Overview Settings")]
    [SerializeField] private Vector3 overviewPosition = new Vector3(0, 15, -10);
    [SerializeField] private Vector3 overviewRotation = new Vector3(45, 0, 0);
    [SerializeField] private float overviewFOV = 60f;

    [Header("Focus Settings")]
    [SerializeField] private Vector3 focusOffset = new Vector3(0, 3, -4);
    [SerializeField] private float focusFOV = 50f;
    [SerializeField] private float lookAheadDistance = 2f;

    private Camera _cam;
    private Checker _currentTarget;
    private Vector3 _lastDirection;

    private void Awake()
    {
        _cam = GetComponent<Camera>();
        ResetToOverview();
    }

    /// <summary>
    /// Устанавливает текущую цель для камеры
    /// </summary>
    public void SetTarget(Checker target)
    {
        if (target != null)
        {
            _currentTarget = target;
            MoveToTarget();
        }
        else
        {
            _currentTarget = null;
            ResetCamera();
        }
      
    }

    /// <summary>
    /// Сбрасывает камеру в обзорный режим
    /// </summary>
    private void ResetCamera()
    {
        _currentTarget = null;
        ReturnToOverview();
    }

    private void Update()
    {
        if (!_currentTarget) return;

        UpdateCameraPosition();
        UpdateCameraRotation();
    }

    private void MoveToTarget()
    {
        var targetTransform = _currentTarget.transform;
        var targetPosition = targetTransform.position + focusOffset;

        transform.DOKill();
        DOTween.Sequence()
            .Append(transform.DOMove(targetPosition, transitionDuration))
            .Join(_cam.DOFieldOfView(focusFOV, transitionDuration))
            .SetEase(transitionEase);
    }

    private void UpdateCameraPosition()
    {
        // Плавное сопровождение цели
        Vector3 targetPosition = _currentTarget.transform.position + focusOffset;
        transform.position = Vector3.Lerp(
            transform.position, 
            targetPosition, 
            Time.deltaTime * 5f
        );
    }

    private void UpdateCameraRotation()
    {
        // Определяем направление с учетом ForceDirection
        Vector3 forceDirection = _currentTarget.ForceDirection;
        if (forceDirection.magnitude > 0.1f)
        {
            _lastDirection = forceDirection.normalized;
        }

        Vector3 lookPoint = _currentTarget.transform.position + 
                          _lastDirection * lookAheadDistance;

        Quaternion targetRotation = Quaternion.LookRotation(lookPoint - transform.position);
        transform.rotation = Quaternion.Slerp(
            transform.rotation, 
            targetRotation, 
            Time.deltaTime * 5f
        );
    }

    private void ReturnToOverview()
    {
        transform.DOKill();
        DOTween.Sequence()
            .Append(transform.DOMove(overviewPosition, transitionDuration))
            .Join(transform.DORotate(overviewRotation, transitionDuration))
            .Join(_cam.DOFieldOfView(overviewFOV, transitionDuration))
            .SetEase(transitionEase);
    }

    private void ResetToOverview()
    {
        transform.position = overviewPosition;
        transform.eulerAngles = overviewRotation;
        _cam.fieldOfView = overviewFOV;
    }
}