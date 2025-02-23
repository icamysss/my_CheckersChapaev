using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField, Tooltip("Ограничения камеры по доске")] 
    private Сonstraints сonstraints ;
    
    [BoxGroup("Debug")]
    [ShowInInspector, ReadOnly] private Checker currentTarget;  // текущая цель
    
    private Tweener moveTween;
    private Tweener lookTween;
    private Tweener rotateTween;
    
    
    private void Start()
    {
        SetOverviewAsStartPosition();
    }

    public void SetTarget(Checker checker)
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

    private void SetOverviewAsStartPosition()
    {
        overviewPosition = transform.position;
        overviewRotation = transform.rotation;
    }

    private void KillActiveTweens()
    {
        moveTween?.Kill();
        lookTween?.Kill();
        rotateTween?.Kill();
    }

    private void ReturnToOverview(float time)
    {
        moveTween = transform.DOMove(overviewPosition, time);
        rotateTween = transform.DORotateQuaternion(overviewRotation, time);
    }

    private void MoveToTarget(Transform target)
    {
        var targetPosition = currentTarget.transform.position;
        // targetPosition.x  = Mathf.Clamp(targetPosition.x , сonstraints.minx, сonstraints.maxX);
        // targetPosition.z  = Mathf.Clamp(targetPosition.z , сonstraints.minZ, сonstraints.minZ);
        // перемещение камеры на шашку
        moveTween = transform.DOMove(targetPosition, moveDuration);
        // поворот камере к центру
        if (targetPosition == overviewPosition) return; // если шашка в центре
        var targetRotation = Quaternion.LookRotation(overviewPosition - target.position);
        lookTween = transform.DORotateQuaternion(targetRotation, moveDuration);
    }
}

[Serializable]
public class Сonstraints
{
    public float maxX, minx, maxZ, minZ;
}