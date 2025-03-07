using System;
using Common;
using Core;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Services
{
    /// <summary>
    /// Управляет движением и поворотом камеры в зависимости от выбранной шашки или обзорного режима.
    /// </summary>
    public class CameraController : MonoBehaviour, ICameraController
    {
        #region Fields
    
        [BoxGroup("Tracking Settings")]
        [SerializeField, Tooltip("Время перемещения к выбранной шашке")]
        private float moveDuration = 1f;

        [BoxGroup("Tracking Settings")]
        [SerializeField, Tooltip("Время возвращения к обзорному режиму")]
        private float backDuration = 0.5f;
        
        [BoxGroup("Tracking Settings")]
        [SerializeField, Tooltip("Анимация передвижения ")]
        private Ease moveEase = Ease.InCubic;

        [BoxGroup("Tracking Settings")]
        [SerializeField, Tooltip("Минимальные значения позиции камеры")]
        private CameraOffset minCamPosition;
    
        [BoxGroup("Tracking Settings")]
        [SerializeField, Tooltip("Максимальные значения позиции камеры")]
        private CameraOffset maxCamPosition;

   

        [BoxGroup("Debug")]
        [ShowInInspector, ReadOnly]
        private Pawn currentTarget; // Текущая выбранная шашка

        [BoxGroup("Debug")]
        [SerializeField, ReadOnly]
        private Camera mainCamera; // Ссылка на основную камеру

        [BoxGroup("Debug")]
        [SerializeField, ReadOnly]
        private CameraOffset defaultCamOffset; // Начальная позиция камеры
    
        [BoxGroup("Debug")]
        [SerializeField, Tooltip("максимальная дистанци от шашки до центра доски"), ReadOnly]
        private float maxDistance;  
    
        [BoxGroup("Debug")]
        [SerializeField, Tooltip("Позиция камеры в обзорном режиме"), ReadOnly]
        private Vector3 overviewPosition;

        [BoxGroup("Debug")]
        [SerializeField, Tooltip("Поворот камеры в обзорном режиме"),ReadOnly]
        private Quaternion overviewRotation;
    
        private Tweener moveTween;
        private Tweener lookTween;
        private Tweener rotateTween;
        private Tweener moveCamTween;
        private Tweener lookCamTween;

        private Board board;

        #endregion

        #region Unity Methods

        /// <summary>
        /// Вызывается при отключении объекта. Отписывается от событий и завершает активные твины.
        /// </summary>
        private void OnDisable()
        {
            Pawn.OnSelect -= SetTarget;
            Pawn.OnForceApplied -= SetTarget;
            ServiceLocator.OnAllServicesRegistered -= OnAllServicesRegistered;
            KillActiveTweens();
        }

        /// <summary>
        /// Вызывается после регистрации всех сервисов. Получает ссылку на доску.
        /// </summary>
        private void OnAllServicesRegistered()
        {
            board = ServiceLocator.Get<IGameManager>().CurrentGame.Board;
            if (board == null)
            {
                Debug.LogError("Board not found");
            }
            Debug.Log($"BoardSize: {board.BoardSize}");
            maxDistance = (board.BoardSize / 2f) * Mathf.Sqrt(2);
        }

        /// <summary>
        /// Вызывается при валидации в редакторе. Проверяет наличие камеры.
        /// </summary>
        private void OnValidate()
        {
            if (mainCamera == null) mainCamera = GetComponentInChildren<Camera>();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Устанавливает цель для отслеживания камерой.
        /// </summary>
        /// <param name="pawn">Выбранная шашка или null для возврата в обзорный режим.</param>
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

        /// <summary>
        /// Устанавливает начальные позиции и повороты для обзорного режима и камеры.
        /// </summary>
        private void SetDefaultPositions()
        {
            // позиция для родителя
            overviewPosition = transform.position;
            overviewRotation = transform.rotation;
       
            // позиция для камеры стартовая
            defaultCamOffset.HeightY = mainCamera.transform.localPosition.y;
            defaultCamOffset.OffsetZ = mainCamera.transform.localPosition.z;
            defaultCamOffset.RotationX = mainCamera.transform.localEulerAngles.x;
        }

        /// <summary>
        /// Завершает все активные анимации (твины).
        /// </summary>
        private void KillActiveTweens()
        {
            moveTween?.Kill();
            lookTween?.Kill();
            rotateTween?.Kill();
            moveCamTween?.Kill();
            lookCamTween?.Kill();
        }

        /// <summary>
        /// Возвращает камеру в обзорную позицию.
        /// </summary>
        /// <param name="time">Длительность анимации возвращения.</param>
        private void ReturnToOverview(float time)
        {
            moveTween = transform.DOMove(overviewPosition, time)
                .SetEase(moveEase);
            
            rotateTween = transform.DORotateQuaternion(overviewRotation, time)
                .SetEase(moveEase);
            
            moveCamTween = mainCamera.transform.DOLocalMove(defaultCamOffset.Position, time)
                .SetEase(moveEase);
            
            lookCamTween = mainCamera.transform.DOLocalRotateQuaternion(defaultCamOffset.Rotation, time)
                .SetEase(moveEase);
        }

        /// <summary>
        /// Перемещает камеру для отслеживания указанной цели.
        /// </summary>
        /// <param name="target">Трансформ цели (шашки).</param>
        private void MoveToTarget(Transform target)
        {
            if (target == null) return;

            var targetPosition = target.position;
            moveTween = transform.DOMove(targetPosition, moveDuration).SetEase(Ease.InOutQuad);

            if (targetPosition != overviewPosition)
            {
                var targetRotation = Quaternion.LookRotation(overviewPosition - targetPosition);
                lookTween = transform.DORotateQuaternion(targetRotation, moveDuration).SetEase(Ease.InOutQuad);
            }

            // Вычисление локальной позиции камеры с учётом расстояния до центра доски
            var distance = Vector3.Distance(targetPosition, board.CenterPosition);
            var factor = Mathf.Clamp01(distance / maxDistance);
            var targetZ = Mathf.Lerp(minCamPosition.OffsetZ, maxCamPosition.OffsetZ, factor);
            var targetY = Mathf.Lerp(minCamPosition.HeightY, maxCamPosition.HeightY, factor);
            var camLocalPosition = new Vector3(0, targetY, targetZ);
        
            moveCamTween = mainCamera.transform.DOLocalMove(camLocalPosition, moveDuration).SetEase(Ease.InOutQuad);

            // Вычисление локального поворота камеры с учётом расстояния до центра доски
            var camRotation = Quaternion.Lerp(minCamPosition.Rotation, maxCamPosition.Rotation, factor);
        
            lookCamTween = mainCamera.transform.DOLocalRotateQuaternion(camRotation, moveDuration).
                SetEase(Ease.InOutQuad);
        }

        #endregion

        #region ICameraController

        /// <summary>
        /// Получает длительность перемещения камеры к цели.
        /// </summary>
        public float MoveDuration => moveDuration;

        /// <summary>
        /// Получает основную камеру.
        /// </summary>
        public Camera MainCamera => mainCamera;

        #endregion

        #region IService

        /// <summary>
        /// Инициализирует контроллер камеры.
        /// </summary>
        public void Initialize()
        {
            mainCamera = GetComponentInChildren<Camera>();
            if (mainCamera == null) throw new NullReferenceException("Main camera not found");

            SetDefaultPositions();

            Pawn.OnSelect += SetTarget;
            Pawn.OnForceApplied += SetTarget;
            ServiceLocator.OnAllServicesRegistered += OnAllServicesRegistered;

            isInitialized = true;
        }

        /// <summary>
        /// Завершает работу контроллера камеры.
        /// </summary>
        public void Shutdown()
        {
            Pawn.OnSelect -= SetTarget;
            Pawn.OnForceApplied -= SetTarget;
            ServiceLocator.OnAllServicesRegistered -= OnAllServicesRegistered;
        }

        /// <summary>
        /// Указывает, инициализирован ли сервис.
        /// </summary>
        public bool isInitialized { get; private set; }

        #endregion
    }
}