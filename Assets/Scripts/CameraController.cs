using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [BoxGroup("Overview Settings")]
    [SerializeField] private Vector3 overviewPosition;
    [BoxGroup("Overview Settings")]
    [SerializeField] private Quaternion overviewRotation;

    [BoxGroup("Tracking Settings")]
    [SerializeField, Tooltip("Время перемещения к выбранной шашке")] 
    private float moveDuration = 1f;
    
    [BoxGroup("Tracking Settings")]
    [SerializeField, Tooltip("Время перемещения к стартовому обзору")] 
    private float backDuration = 0.5f;
   
    [BoxGroup("Tracking Settings")]
    [SerializeField, Tooltip("Позиция камеры во время прицеливания")] 
    private Vector3 TrackingCamPosition ;
    [BoxGroup("Tracking Settings")]
    [SerializeField, Tooltip("Позиция камеры во время прицеливания")] 
    private Quaternion TrackingCamRotation ;
    
    [BoxGroup("Debug")]
    [ShowInInspector, ReadOnly] private Checker currentTarget;  // текущая цель
    [BoxGroup("Debug")]
    [SerializeField, ReadOnly] private Camera mainCamera;
    [BoxGroup("Debug")]
    [SerializeField, ReadOnly] private Vector3 defaultCamPosition;
    [BoxGroup("Debug")]
    [SerializeField, ReadOnly] private Quaternion defaultCamRotation;
    
    
    
    
    private Tweener moveTween;
    private Tweener lookTween;
    private Tweener rotateTween;
    
    private Tweener moveCamTween;
    private Tweener lookCamTween;


    private void OnEnable()
    {
        Checker.OnSelect += SetTarget;
        Checker.OnDeselect += SetTarget; // передает null в качестве аргумента
    }
    
    private void OnDisable()
    {
        Checker.OnSelect -= SetTarget;
        Checker.OnDeselect -= SetTarget;
    }

    private void Start()
    {
        mainCamera = GetComponentInChildren<Camera>();
        if (mainCamera == null) throw new NullReferenceException("mainCamera not found");
        
        SetDefaultPositions();
    }

    private void OnValidate()
    {
        if (mainCamera == null) mainCamera = GetComponentInChildren<Camera>();
    }

    private void SetTarget(Checker checker)
    {
        currentTarget = checker;
        KillActiveTweens();

        if (currentTarget == null)
        {
            ReturnToOverview(backDuration);
        }
        else
        {
            MoveToTarget(currentTarget.transform);
        }
    }

    private void SetDefaultPositions()
    {
        overviewPosition = transform.position;
        overviewRotation = transform.rotation;
        
        defaultCamPosition = mainCamera.transform.position;
        defaultCamRotation = mainCamera.transform.rotation;
    }

    private void KillActiveTweens()
    {
        moveTween?.Kill();
        lookTween?.Kill();
        rotateTween?.Kill();
        moveCamTween?.Kill();
        lookCamTween?.Kill();
    }

    private void ReturnToOverview(float time)
    {
        // возвращаем родителя на место
        moveTween = transform.DOMove(overviewPosition, time);
        rotateTween = transform.DORotateQuaternion(overviewRotation, time);
        // возвращаем камеру на место
        moveCamTween = mainCamera.transform.DOMove(defaultCamPosition, time);
        lookCamTween = mainCamera.transform.DORotateQuaternion(defaultCamRotation, time);
    }

    private void MoveToTarget(Transform target)
    {
        // -------- Изменение положение родителя камеры
        var targetPosition = currentTarget.transform.position;
        // перемещение камеры на шашку
        moveTween = transform.DOMove(targetPosition, moveDuration);
        // поворот камеры к центру
        if (targetPosition == overviewPosition) return; // если шашка в центре
        var targetRotation = Quaternion.LookRotation(overviewPosition - target.position);
        lookTween = transform.DORotateQuaternion(targetRotation, moveDuration);
        
        // -------- Изменение положение камеры (Main Camera)
        if (mainCamera.transform.position != TrackingCamPosition)
        {
            moveCamTween = mainCamera.transform.DOLocalMove(TrackingCamPosition, moveDuration);
        }
        if (mainCamera.transform.rotation != TrackingCamRotation)
        {
            lookCamTween = mainCamera.transform.DOLocalRotateQuaternion(TrackingCamRotation, moveDuration); 
        }
    }
    
}