using System.Collections.Generic;
using Core;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Services;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AI
{
    public class AIController
    {
        #region Private Variables

        private List<Pawn> aiPawns = new();
        private List<Pawn> enemyPawns = new();
        private Pawn aiSelectedPawn;
        private ScoreCalculator scoreCalculator;
        private Board board;
        private ICameraController cameraController;
        private AISettings aiSettings;
        private Vector3 finalShotDirection;

        private Tween currenTween;

        #endregion

        #region Initialization

        private void OnAllServicesReady()
        {
            cameraController = ServiceLocator.Get<ICameraController>();
        }
        /// <summary>
        /// Инициализация контроллера ИИ с доской и камерой
        /// </summary>
        public void Initialize(Board instantiatedBoard)
        {
            board = instantiatedBoard;
            
            aiSettings = new AISettings();
            ServiceLocator.OnAllServicesRegistered += OnAllServicesReady;
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
            if (pl.AISettings != null)
            {
                aiSettings = pl.AISettings;
                scoreCalculator = new ScoreCalculator(aiSettings, this.board.BoardSize);
            }
            RefreshPawnLists(pl.PawnColor);
            AIMove().Forget();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Обновление списков шашек ИИ и врагов
        /// </summary>
        private void RefreshPawnLists(PawnColor pawnColor)
        {
            aiPawns = board.GetPawnsOnBoard(pawnColor);
            enemyPawns = board.GetPawnsOnBoard(pawnColor == PawnColor.Black ? PawnColor.White : PawnColor.Black);
        }

        /// <summary>
        /// Основная корутина хода ИИ
        /// </summary>
        private async UniTask AIMove()
        {
            // 1. Думает 1-3 секунды
            var decisionDelay = Random.Range(aiSettings.MinDecisionDelay, aiSettings.MaxDecisionDelay);
            await UniTask.Delay(decisionDelay);

            // 2. Выбирает оптимальную шашку и вызывает Select()
            aiSelectedPawn = SelectOptimalPawn();
            if (aiSelectedPawn == null) return;
            aiSelectedPawn.Select();

            // 3. Ждет moveDuration + 0.5 секунды для завершения движения камеры
            var waitTime = cameraController.MoveDuration + 500;
            await UniTask.Delay(waitTime);

            // 4. Имитация прицеливания
            await AimSimulate();

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

                float score = scoreCalculator.Calculate(
                    pawn.transform.position,
                    aiPawns,
                    enemyPawns,
                    board.BoardSize
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
        private async UniTask AimSimulate()
        {
            var optimalDirection = CalculateOptimalDirection(aiSelectedPawn);
            // Оптимальное направление для выстрела
            var currentDirection = optimalDirection + Random.insideUnitSphere * 0.5f;
            // Начальное случайное отклонение
            currentDirection.y = 0; // Ограничиваем движение по горизонтали
            currentDirection.Normalize();

            // Настройки колебаний
            var oscillationCount = Random.Range(2,6); // Количество небольших корректировок
            var totalAimingTime = aiSettings.AimingTime/ 1000f - 0.5f; // Оставляем 0.5 сек на финальную фиксацию
            var oscillationDuration = totalAimingTime / oscillationCount; // Время на каждую фазу колебаний

            // Этап 1: Колебания (имитация неуверенности или корректировки)
            for (var i = 0; i < oscillationCount; i++)
            {
                var targetDirection = optimalDirection + Random.insideUnitSphere * 0.2f;
                // Небольшое отклонение от цели
                targetDirection.y = 0;
                targetDirection.Normalize();

                
                currenTween?.Kill();
                // Плавный переход к новому направлению
                currenTween = DOTween.To(() => currentDirection, x => currentDirection = x, targetDirection,
                    oscillationDuration)
                    .SetEase(Ease.InOutQuad)
                    .OnUpdate(() =>
                    {
                        var force = CalculateForce(aiSelectedPawn);
                        aiSelectedPawn.UpdateLineVisuals(force, currentDirection); // Обновляем визуализацию
                    })
                    .OnKill(() => currenTween = null);
                await currenTween.AsyncWaitForCompletion();
            }

            // Этап 2: Фиксация на оптимальном направлении
            currenTween = DOTween.To(() => currentDirection, x => currentDirection = x, optimalDirection, 0.5f)
                .SetEase(Ease.InOutQuad)
                .OnUpdate(() =>
                {
                    var force = CalculateForce(aiSelectedPawn);
                    aiSelectedPawn.UpdateLineVisuals(force, currentDirection); // Обновляем визуализацию
                })
                .OnComplete(() =>
                {
                    // Финальное обновление визуализации перед выстрелом
                    var finalForce = CalculateForce(aiSelectedPawn);
                    aiSelectedPawn.UpdateLineVisuals(finalForce, optimalDirection);
                    finalShotDirection = optimalDirection;
                }).OnKill(()=> currenTween = null);
            await currenTween.AsyncWaitForCompletion();
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

            var targetPosition = scoreCalculator.FindOptimalTarget(
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
            var distance = Vector3.Distance(pawn.transform.position, scoreCalculator.LastCalculatedTarget);
            var forceMultiplier = Mathf.Clamp01(distance / board.BoardSize);
            var force = Mathf.Lerp(pawn.minForce, pawn.maxForce, forceMultiplier);
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