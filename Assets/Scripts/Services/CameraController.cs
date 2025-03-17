using System;
using System.Threading;
using Common;
using Core;
using DG.Tweening;
using Services.Interfaces;
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

        [BoxGroup("Tracking Settings")] [SerializeField, Tooltip("Время перемещения к выбранной шашке")] private int
            moveDurationMS = 1500;

        [BoxGroup("Tracking Settings")] [SerializeField, Tooltip("Время возвращения к обзорному режиму")] private int
            backDurationMS = 500;

        [BoxGroup("Tracking Settings")] [SerializeField, Tooltip("Анимация передвижения ")] private Ease moveEase =
            Ease.InOutQuad;
        [BoxGroup("Tracking Settings")] [SerializeField, Tooltip("Анимация возвращения ")] private Ease moveBack =
            Ease.InOutCubic;

        [BoxGroup("Tracking Settings")] [SerializeField, Tooltip("Минимальные значения позиции камеры")] private
            CameraOffset minCamPosition;

        [BoxGroup("Tracking Settings")] [SerializeField, Tooltip("Максимальные значения позиции камеры")] private
            CameraOffset maxCamPosition;


        [BoxGroup("Debug")] [ShowInInspector, ReadOnly] private Pawn currentTarget; // Текущая выбранная шашка

        [BoxGroup("Debug")] [SerializeField, ReadOnly] private Camera mainCamera; // Ссылка на основную камеру

        [BoxGroup("Debug")] [SerializeField, ReadOnly] private CameraOffset defaultCamOffset;
            // Начальная позиция камеры

        [BoxGroup("Debug")] [SerializeField, Tooltip("максимальная дистанци от шашки до центра доски"), ReadOnly] private float maxDistance;

        [BoxGroup("Debug")] [SerializeField, Tooltip("Позиция камеры в обзорном режиме"), ReadOnly] private Vector3
            overviewPosition;

        [BoxGroup("Debug")] [SerializeField, Tooltip("Поворот камеры в обзорном режиме"), ReadOnly] private Quaternion
            overviewRotation;
        
        private Sequence moveSequence;

        private Board board;

        #endregion

        #region Unity Methods

        /// <summary>
        /// Вызывается при отключении объекта. Отписывается от событий и завершает активные твины.
        /// </summary>
        private void OnDisable()
        {
            Pawn.OnSelect -= SetTarget;
            Pawn.OnKickPawn -= ReturnToOverview;
            ServiceLocator.OnAllServicesRegistered -= OnAllServicesRegistered;
            moveSequence.Kill();
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
            
            if (currentTarget == null)
            {
                ReturnToOverview();
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
        /// Возвращает камеру в обзорную позицию.
        /// </summary>
        private void ReturnToOverview()
        {
            moveSequence?.Kill();
            moveSequence = DOTween.Sequence();

            // ------------ родителя возвращаем  -----------
            moveSequence.Join(transform.DOMove(overviewPosition, backDurationMS/1000f)
                .SetEase(moveBack));
            moveSequence.Join(transform.DORotateQuaternion(overviewRotation, backDurationMS/1000f)
                .SetEase(moveBack));
            // ----------- камеру возвращаем -----------
            moveSequence.Join(mainCamera.transform.DOLocalMove(defaultCamOffset.Position, backDurationMS/1000f)
                .SetEase(moveBack));
            moveSequence.Join(mainCamera.transform.DOLocalRotateQuaternion(defaultCamOffset.Rotation, backDurationMS/1000f)
                .SetEase(moveBack));
            moveSequence.Play();
        }

        /// <summary>
        /// Перемещает камеру для отслеживания указанной цели.
        /// </summary>
        /// <param name="target">Трансформ цели (шашки).</param>
        private void MoveToTarget(Transform target)
        {
            if (target == null) return;
            moveSequence.Kill();
            moveSequence = DOTween.Sequence();
            
            var targetPosition = target.position;
           
            
            moveSequence.Join(transform.DOMove(targetPosition, moveDurationMS / 1000f).SetEase(moveEase));
           

            if (targetPosition != overviewPosition)
            {
                var targetRotation = Quaternion.LookRotation(overviewPosition - targetPosition);
                moveSequence.Join(transform.DORotateQuaternion(targetRotation, moveDurationMS / 1000f)
                    .SetEase(moveEase));
            }

            // Вычисление локальной позиции камеры с учётом расстояния до центра доски
            var distance = Vector3.Distance(targetPosition, board.CenterPosition);
            var factor = Mathf.Clamp01(distance / maxDistance);
            var targetZ = Mathf.Lerp(minCamPosition.OffsetZ, maxCamPosition.OffsetZ, factor);
            var targetY = Mathf.Lerp(minCamPosition.HeightY, maxCamPosition.HeightY, factor);
            var camLocalPosition = new Vector3(0, targetY, targetZ);

            moveSequence.Join(mainCamera.transform.DOLocalMove(camLocalPosition, moveDurationMS / 1000f).SetEase(moveEase));
            // Вычисление локального поворота камеры с учётом расстояния до центра доски
            var camRotation = Quaternion.Lerp(minCamPosition.Rotation, maxCamPosition.Rotation, factor);
            moveSequence.Join(mainCamera.transform.DOLocalRotateQuaternion(camRotation, moveDurationMS / 1000f).SetEase(moveEase));
            
            moveSequence.Play();
        }

        #endregion

        #region ICameraController

        /// <summary>
        /// Получает длительность перемещения камеры к цели.
        /// </summary>
        public int MoveDurationMS => moveDurationMS;

        /// <summary>
        /// Получает основную камеру.
        /// </summary>
        public Camera MainCamera => mainCamera;

        #endregion

        /// <summary>
        /// Вызывается после регистрации всех сервисов. Получает ссылку на доску.
        /// </summary>
        private void OnAllServicesRegistered()
        {
            board = ServiceLocator.Get<IGameManager>().CurrentGame.Board;
            maxDistance = (board.BoardSize / 2f) * Mathf.Sqrt(2);
        }

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
            Pawn.OnKickPawn += ReturnToOverview;
            ServiceLocator.OnAllServicesRegistered += OnAllServicesRegistered;

            IsInitialized = true;
        }

        /// <summary>
        /// Завершает работу контроллера камеры.
        /// </summary>
        public void Shutdown()
        {
            // Pawn.OnSelect -= SetTarget;
            // Pawn.OnKickPawn -= SetTarget;
            ServiceLocator.OnAllServicesRegistered -= OnAllServicesRegistered;
        }

        /// <summary>
        /// Указывает, инициализирован ли сервис.
        /// </summary>
        public bool IsInitialized { get; private set; }

        #endregion
    }
}