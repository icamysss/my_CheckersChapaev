using System.Collections;
using System.Collections.Generic;
using Core;
using DG.Tweening;
using Services;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AI
{
    public class AIController : MonoBehaviour
    {
        #region Inspector Variables

        [Header("Timing Settings")] [SerializeField, Tooltip("Минимальное время принятия решения (сек)")] private float
            minDecisionDelay = 1f;

        [SerializeField, Tooltip("Максимальное время принятия решения (сек)")] private float maxDecisionDelay = 4f;
        [SerializeField, Tooltip("Время имитации прицеливания (сек)")] private float aimingTime = 2f;

        [Header("Score Calculation Settings")] [SerializeField, Tooltip("Радиус поиска соседних шашек")] private float
            neighborRadius = 1.0f;

        [Header("Score Weights")] [SerializeField, Tooltip("Вес группировки своих шашек")] private float
            friendlyGroupWeight = 0.7f;

        [SerializeField, Tooltip("Вес близости к вражеским шашкам")] private float enemyProximityWeight = 1.2f;
        [SerializeField, Tooltip("Вес линии огня (потенциальных попаданий)")] private float lineOfFireWeight = 2.0f;
        [SerializeField, Tooltip("Вес позиции ближе к центру доски")] private float boardCenterWeight = 0.5f;
        [SerializeField, Tooltip("Штраф за близость к краям доски")] private float edgePenaltyWeight = 0.3f;

        #endregion

        #region Private Variables

        public List<Pawn> aiPawns = new();
        public List<Pawn> enemyPawns = new();

        [ShowInInspector, ReadOnly] private Pawn aiSelectedPawn;
        private ScoreCalculator _scoreCalculator;
        private Board _board;
        private ICameraController _cameraController;

        private Vector3 finalShotDirection;
        #endregion

        #region Initialization

        private void OnEnable()
        {
            ServiceLocator.OnAllServicesRegistered += AllServicesRegistered;
        }

        private void AllServicesRegistered()
        {
            var board = ServiceLocator.Get<IGameManager>().CurrentGame.Board;
            Initialize(board);
        }

        /// <summary>
        /// Инициализация контроллера ИИ с доской и камерой
        /// </summary>
        private void Initialize(Board board)
        {
            _board = board;
            _cameraController = ServiceLocator.Get<ICameraController>();
            _scoreCalculator = new ScoreCalculator(neighborRadius, _board.BoardSize);
            SetScoreWeights();
        }

        /// <summary>
        /// Установка весов в ScoreCalculator из значений в инспекторе
        /// </summary>
        private void SetScoreWeights()
        {
            _scoreCalculator.FriendlyGroupWeight = friendlyGroupWeight;
            _scoreCalculator.EnemyProximityWeight = enemyProximityWeight;
            _scoreCalculator.LineOfFireWeight = lineOfFireWeight;
            _scoreCalculator.BoardCenterWeight = boardCenterWeight;
            _scoreCalculator.EdgePenaltyWeight = edgePenaltyWeight;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Запуск хода ИИ для указанного цвета шашек
        /// </summary>
        public void MakeMove(Player pl)
        {
            if (pl.PawnColor == PawnColor.None)
            {
                Debug.LogError($"Invalid pawn color: {pl.PawnColor}");
                return;
            }

            RefreshPawnLists(pl.PawnColor);
            StartCoroutine(AIMoveRoutine());
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Обновление списков шашек ИИ и врагов
        /// </summary>
        private void RefreshPawnLists(PawnColor pawnColor)
        {
            aiPawns = _board.GetPawnsOnBoard(pawnColor);
            enemyPawns = _board.GetPawnsOnBoard(pawnColor == PawnColor.Black ? PawnColor.White : PawnColor.Black);
        }

        /// <summary>
        /// Основная корутина хода ИИ
        /// </summary>
        private IEnumerator AIMoveRoutine()
        {
            // 1. Думает 1-3 секунды
            float decisionDelay = Random.Range(minDecisionDelay, maxDecisionDelay);
            yield return new WaitForSeconds(decisionDelay);

            // 2. Выбирает оптимальную шашку и вызывает Select()
            aiSelectedPawn = SelectOptimalPawn();
            if (aiSelectedPawn == null) yield break;
            aiSelectedPawn.Select();

            // 3. Ждет moveDuration + 0.5 секунды для завершения движения камеры
            float waitTime = _cameraController.MoveDuration + 0.5f;
            yield return new WaitForSeconds(waitTime);

            // 4. Имитация прицеливания
            yield return StartCoroutine(AimRoutine());

            // 5. Применение силы и завершение хода
            var force = CalculateForce(aiSelectedPawn);
            aiSelectedPawn.ApplyForce(finalShotDirection * force);
        }

        /// <summary>
        /// Выбор оптимальной шашки на основе скоринга
        /// </summary>
        private Pawn SelectOptimalPawn()
        {
            Pawn bestPawn = null;
            float maxScore = float.MinValue;

            foreach (Pawn pawn in aiPawns)
            {
                if (pawn == null) continue;

                float score = _scoreCalculator.Calculate(
                    pawn.transform.position,
                    aiPawns,
                    enemyPawns,
                    _board.BoardSize
                    );

                if (score > maxScore)
                {
                    maxScore = score;
                    bestPawn = pawn;
                }
            }
            return bestPawn;
        }

        /// <summary>
        /// Корутина для имитации прицеливания с поведением, похожим на человеческое
        /// </summary>
        private IEnumerator AimRoutine()
        {
            Vector3 optimalDirection = CalculateOptimalDirection(aiSelectedPawn);
                // Оптимальное направление для выстрела
            Vector3 currentDirection = optimalDirection + Random.insideUnitSphere * 0.5f;
                // Начальное случайное отклонение
            currentDirection.y = 0; // Ограничиваем движение по горизонтали
            currentDirection.Normalize();

            // Настройки колебаний
            int oscillationCount = 3; // Количество небольших корректировок
            float totalAimingTime = aimingTime - 0.5f; // Оставляем 0.5 сек на финальную фиксацию
            float oscillationDuration = totalAimingTime / oscillationCount; // Время на каждую фазу колебаний

            // Этап 1: Колебания (имитация неуверенности или корректировки)
            for (int i = 0; i < oscillationCount; i++)
            {
                Vector3 targetDirection = optimalDirection + Random.insideUnitSphere * 0.2f;
                    // Небольшое отклонение от цели
                targetDirection.y = 0;
                targetDirection.Normalize();

                // Плавный переход к новому направлению
                yield return
                    DOTween.To(() => currentDirection, x => currentDirection = x, targetDirection, oscillationDuration)
                        .SetEase(Ease.InOutQuad)
                        .OnUpdate(() =>
                        {
                            float force = CalculateForce(aiSelectedPawn);
                            aiSelectedPawn.UpdateLineVisuals(force, currentDirection); // Обновляем визуализацию
                        })
                        .WaitForCompletion();
            }

            // Этап 2: Фиксация на оптимальном направлении
            yield return DOTween.To(() => currentDirection, x => currentDirection = x, optimalDirection, 0.5f)
                .SetEase(Ease.InOutQuad)
                .OnUpdate(() =>
                {
                    float force = CalculateForce(aiSelectedPawn);
                    aiSelectedPawn.UpdateLineVisuals(force, currentDirection); // Обновляем визуализацию
                })
                .OnComplete(() =>
                {
                    // Финальное обновление визуализации перед выстрелом
                    float finalForce = CalculateForce(aiSelectedPawn);
                    aiSelectedPawn.UpdateLineVisuals(finalForce, optimalDirection);
                    finalShotDirection = optimalDirection;
                })
                .WaitForCompletion();

            // Теперь выстрел будет произведён в optimalDirection
        }

        /// <summary>
        /// Расчет оптимального направления выстрела
        /// </summary>
        private Vector3 CalculateOptimalDirection(Pawn selectedPawn)
        {
            if (enemyPawns.Count == 0)
            {
                Debug.LogWarning("No enemies available for targeting");
                return GetFallbackDirection(selectedPawn.transform);
            }

            Vector3 targetPosition = _scoreCalculator.FindOptimalTarget(
                selectedPawn.transform.position,
                enemyPawns
                );

            if (Vector3.Distance(targetPosition, selectedPawn.transform.position) < 0.1f)
            {
                targetPosition = GetFallbackTarget(selectedPawn.transform.position);
            }

            Vector3 direction = (targetPosition - selectedPawn.transform.position).normalized;
            Debug.DrawRay(selectedPawn.transform.position, direction * 2, Color.green, 2f);

            return direction;
        }

        /// <summary>
        /// Расчет силы выстрела на основе расстояния до цели
        /// </summary>
        private float CalculateForce(Pawn pawn)
        {
            float distance = Vector3.Distance(pawn.transform.position, _scoreCalculator.LastCalculatedTarget);
            float forceMultiplier = Mathf.Clamp01(distance / _scoreCalculator.MaxPredictionDistance);
            float force = Mathf.Lerp(pawn.minForce, pawn.maxForce, forceMultiplier);
            return force;
        }

        /// <summary>
        /// Получение запасного направления, если врагов нет
        /// </summary>
        private Vector3 GetFallbackDirection(Transform pawnTransform)
        {
            return pawnTransform.forward + new Vector3(
                Random.Range(-0.3f, 0.3f),
                0,
                Random.Range(0.5f, 1f)
                ).normalized;
        }

        /// <summary>
        /// Получение запасной цели, если оптимальная цель слишком близко
        /// </summary>
        private Vector3 GetFallbackTarget(Vector3 currentPosition)
        {
            if (enemyPawns.Count > 0)
            {
                return enemyPawns[Random.Range(0, enemyPawns.Count)].transform.position;
            }
            return currentPosition + Vector3.forward * 2f;
        }

        #endregion
    }
}