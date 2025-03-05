using System;
using Core;
using DG.Tweening;
using Services;
using Sirenix.OdinInspector;
using UnityEngine;
// TODO Сделать нормальное отображение в испекторе, интерполция позиции камеры от расстояния выбранной шашки до центра, 
// добавить к твинам easy
public class CameraController : MonoBehaviour, ICameraController
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
    private Vector3 trackingCamPosition ;
  
    [BoxGroup("Tracking Settings")]
    [SerializeField, Tooltip("Позиция камеры во время прицеливания")] 
    private Quaternion trackingCamRotation ;
    
    [BoxGroup("Debug")]
    [ShowInInspector, ReadOnly] private Pawn currentTarget;  // текущая цель
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
    
    
    private void OnDisable()
    {
        Pawn.OnSelect -= SetTarget;
        Pawn.OnForceApplied -= SetTarget;
    }
    
    private void OnValidate()
    {
        if (mainCamera == null) mainCamera = GetComponentInChildren<Camera>();
    }

    private void SetTarget(Pawn pawn)
    {
        currentTarget = pawn;
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
        if (mainCamera.transform.position != trackingCamPosition)
        {
            moveCamTween = mainCamera.transform.DOLocalMove(trackingCamPosition, moveDuration);
        }
        if (mainCamera.transform.rotation != trackingCamRotation)
        {
            lookCamTween = mainCamera.transform.DOLocalRotateQuaternion(trackingCamRotation, moveDuration); 
        }
    }

    #region ICameraController

    public float MoveDuration => moveDuration;
    public Camera MainCamera => mainCamera;

    #endregion
    
    
    #region IService
    
    public void Initialize()
    {
        mainCamera = GetComponentInChildren<Camera>();
        if (mainCamera == null) throw new NullReferenceException("mainCamera not found");
        
        SetDefaultPositions();
        
        Pawn.OnSelect += SetTarget;
        Pawn.OnForceApplied += SetTarget; // передает null в качестве аргумента
        
        isInitialized = true;
        
    }

    public void Shutdown()
    {
        Pawn.OnSelect -= SetTarget;
        Pawn.OnForceApplied -= SetTarget;
    }

    public bool isInitialized { get; private set; }
    
    #endregion
   
}