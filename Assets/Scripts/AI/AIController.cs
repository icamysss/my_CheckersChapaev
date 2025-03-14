using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Core;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Services;
using Services.Interfaces;
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
        private Game game;
        private Tween currenTween;

        #endregion

        #region Initialization

        private void OnAllServicesReady()
        {
            cameraController = ServiceLocator.Get<ICameraController>();
        }

        /// <summary>
        /// Инициализация контроллера ИИ с объектом игры
        /// </summary>
        public void Initialize(Game newGame)
        {
            board = newGame.Board;
            game = newGame;
            aiSettings = new AISettings();
            ServiceLocator.OnAllServicesRegistered += OnAllServicesReady;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Запуск хода ИИ для указанного игрока с поддержкой отмены
        /// </summary>
        public async UniTask MakeMove(Player pl, CancellationToken cancellationToken)
        {
            if (pl.PawnColor == PawnColor.None)
            {
                Debug.LogError($"Invalid pawn color: {pl.PawnColor}");
                return;
            }

            aiSettings = pl.AISettings ?? new AISettings();
            scoreCalculator = new ScoreCalculator(pl, board);

            RefreshPawnLists(pl.PawnColor);

            try
            {
                await AIMove(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                Debug.Log("AI move was cancelled.");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"AI Move failed: {ex}");
            }
            finally
            {
                await game.SwitchTurnAsync();
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Обновление списков шашек ИИ и врагов
        /// </summary>
        private void RefreshPawnLists(PawnColor pawnColor)
        {
            aiPawns = board.GetPawnsOnBoard(pawnColor).Where(p => p != null).ToList();
            enemyPawns = board.GetPawnsOnBoard(
                pawnColor == PawnColor.Black ? PawnColor.White : PawnColor.Black
            ).Where(p => p != null).ToList();
        }

        /// <summary>
        /// Основной метод хода ИИ с поддержкой отмены
        /// </summary>
        private async UniTask AIMove(CancellationToken cancellationToken)
        {
            // 1. Задержка для имитации "размышлений" ИИ
            var decisionDelay = Random.Range(aiSettings.MinDecisionDelay, aiSettings.MaxDecisionDelay);
            await UniTask.Delay(decisionDelay, cancellationToken: cancellationToken);

            // 2. Выбор оптимальной шашки и ее активация
            aiSelectedPawn = SelectOptimalPawn();
            if (aiSelectedPawn == null)
                throw new Exception("No valid pawn selected for AI move.");
            aiSelectedPawn.Select();

            // 3. Ожидание завершения движения камеры
            var cameraMoveDelay = cameraController.MoveDuration + aiSettings.TimeAfterCamSetPosition;
            await UniTask.Delay(cameraMoveDelay, cancellationToken: cancellationToken);

            // 4. Имитация прицеливания
            await AimSimulateAsync(cancellationToken);

            // 5. Применение силы для выполнения хода
            var force = CalculateForce(aiSelectedPawn);
            aiSelectedPawn.ApplyForce(finalShotDirection * force);
        }

        /// <summary>
        /// Выбор оптимальной шашки на основе скоринга
        /// </summary>
        private Pawn SelectOptimalPawn()
        {
            Pawn bestPawn = null;
            var maxScore = float.MinValue;

            foreach (var pawn in aiPawns)
            {
                if (pawn == null) continue;

                var score = scoreCalculator.Calculate(
                    pawn.transform.position,
                    aiPawns,
                    enemyPawns
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
        /// Метод имитации прицеливания с поведением, похожим на человеческое
        /// </summary>
        private async UniTask AimSimulateAsync(CancellationToken cancellationToken)
        {// todo добавить колебания по силе удара
            if (aiSelectedPawn == null || enemyPawns.Count == 0)
            {
                finalShotDirection = GetFallbackDirection(aiSelectedPawn?.transform);
                return;
            }

            var optimalDirection = CalculateOptimalDirection(aiSelectedPawn);
            var currentDirection = optimalDirection + Random.insideUnitSphere * 0.5f;
            currentDirection.y = 0;
            currentDirection.Normalize();

            var oscillationCount = Random.Range(2, 6);
            var totalAimingTime = aiSettings.AimingTime / 1000f - 0.5f;
            var oscillationDuration = totalAimingTime / oscillationCount;

            // Этап 1: Колебания для имитации корректировки
            for (var i = 0; i < oscillationCount; i++)
            {
                var targetDirection = optimalDirection + Random.insideUnitSphere * 0.2f;
                targetDirection.y = 0;
                targetDirection.Normalize();

                currenTween?.Kill();
                currenTween = DOTween.To(() => currentDirection, x => currentDirection = x, targetDirection,
                    oscillationDuration)
                    .SetEase(Ease.InOutQuad)
                    .OnUpdate(() =>
                    {
                        var force = CalculateForce(aiSelectedPawn);
                        aiSelectedPawn.UpdateLineVisuals(force, currentDirection);
                    })
                    .OnKill(() => currenTween = null);

                try
                {
                    await currenTween.AsyncWaitForCompletion();
                }
                finally
                {
                    currenTween?.Kill(); // Остановка анимации при отмене
                }
            }
           
            // Этап 2: Финальная фиксация направления
            currenTween = DOTween.To(() => currentDirection, x => currentDirection = x, optimalDirection, 0.5f)
                .SetEase(Ease.InOutQuad)
                .OnUpdate(() =>
                {
                    var force = CalculateForce(aiSelectedPawn);
                    aiSelectedPawn.UpdateLineVisuals(force, currentDirection);
                })
                .OnComplete(() =>
                {
                    var finalForce = CalculateForce(aiSelectedPawn);
                    aiSelectedPawn.UpdateLineVisuals(finalForce, optimalDirection);
                    finalShotDirection = optimalDirection;
                })
                .OnKill(() => currenTween = null);

            try
            {
                if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();
                await currenTween.AsyncWaitForCompletion();
            }
            finally
            {
                currenTween?.Kill(); // Остановка анимации при отмене
            }
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

            enemyPawns.RemoveAll(p => p == null);
            var targetPosition = scoreCalculator.FindOptimalTarget(selectedPawn.transform.position, enemyPawns);

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
            try
            {
                var distance = Vector3.Distance(pawn.transform.position, scoreCalculator.CalculatedTarget);
                var forceMultiplier = Mathf.Clamp01(distance / board.BoardSize);
                var force = Mathf.Lerp(pawn.minForce, pawn.maxForce, forceMultiplier);
                return force;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Force calculation failed: {e}, Returned force = 0");
                return 0f;
            }
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