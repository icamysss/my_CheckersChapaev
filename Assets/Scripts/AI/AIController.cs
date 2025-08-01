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
        private AICalculator aiCalculator;
        private Board board;
        private ICameraController cameraController;
        private AISettings aiSettings;
        private Vector3 shotDirection;
        private float shotPower;
        private Game game;
        private Sequence currenSequence;

        #endregion

        #region Initialization
        

        /// <summary>
        /// Инициализация контроллера ИИ с объектом игры
        /// </summary>
        public void Initialize(Game newGame)
        {
            board = newGame.Board;
            game = newGame;
            aiSettings = new AISettings();
            cameraController ??= ServiceLocator.Get<ICameraController>();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Запуск хода ИИ для указанного игрока с поддержкой отмены
        /// </summary>
        public async UniTask MakeMove(Player pl, CancellationToken cancellationToken)
        {
            if (pl == null || pl.PawnColor == PawnColor.None )
            {
                Debug.LogError("Invalid player, cant make move");
                return;
            }

            try
            {
                // 1. ========= настройка параметров хода =========
                RefreshPawnLists(pl.PawnColor);
                aiCalculator = new AICalculator(pl, board, aiPawns, enemyPawns);
                aiSettings = pl.AISettings ?? new AISettings(); // если настроек ии нет, создаем новые
                aiSelectedPawn = aiCalculator.SelectOptimalPawn();
                shotDirection = aiCalculator.CalculateOptimalDirection(aiSelectedPawn);
                shotPower = aiCalculator.CalculateForce(aiSelectedPawn);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"AI Move failed: {ex}");
                // вызываем событие для смены хода
                Pawn.OnKickPawn?.Invoke();
            }
            
            try
            {
                // 2. ========= Задержка для имитации "размышлений" ИИ =========
                var decisionDelay = Random.Range(aiSettings.MinDecisionDelayMS, aiSettings.MaxDecisionDelayMS);
                await UniTask.Delay(decisionDelay, cancellationToken: cancellationToken);
                aiSelectedPawn.Select(); // выбор шашки, после выбора камера передвигается к выбранной шашке

                // 3. ========= Ожидание завершения движения камеры =========
                var cameraMoveDelay = cameraController.MoveDurationMS + aiSettings.TimeAfterCamSetPositionMS;
                await UniTask.Delay(cameraMoveDelay, cancellationToken: cancellationToken);

                // 4. ========== Имитация прицеливания ================
                await AimSimulateAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                Debug.Log("AI move was cancelled. Simulation failed");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"AI Move simulation failed: {e}");
            }
            finally
            {
                // 5. ========== Применение силы для выполнения хода ============
                aiSelectedPawn.ApplyForce(shotDirection * shotPower);
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
        /// Метод имитации прицеливания с поведением, похожим на человеческое
        /// </summary>
        private async UniTask AimSimulateAsync(CancellationToken cancellationToken)
        {
            var currentDirection = shotDirection + Random.insideUnitSphere * 0.2f;
            currentDirection.y = 0;
            currentDirection.Normalize();
            
            var oscillationCount = Random.Range(2, 6);
            var totalAimingTime = aiSettings.AimingTimeMS / 1000f - 0.5f;
            var oscillationDuration = totalAimingTime / oscillationCount;

            var currentForce = 0f;

            #region 1. Колебания для имитации корректировки 

            for (var i = 0; i < oscillationCount; i++)
            {
                if (cancellationToken.IsCancellationRequested) break;

                var targetDirection = shotDirection + Random.insideUnitSphere * 0.2f;
                targetDirection.y = 0;
                targetDirection.Normalize();

                currenSequence?.Kill();
                currenSequence = DOTween.Sequence();
                currenSequence.Join(DOTween.To(
                    getter: () => currentForce,
                    setter: x => currentForce = x,
                    endValue: shotPower,
                    duration: oscillationDuration
                    ).SetEase(Ease.OutBounce));

                currenSequence.Join(DOTween.To(() => currentDirection, x => currentDirection = x, targetDirection,
                    oscillationDuration)
                    .SetEase(Ease.InOutQuad)
                    .OnUpdate(() => { aiSelectedPawn.UpdateLineVisuals(currentForce, currentDirection); })
                    .OnKill(() => currenSequence = null));

                try
                {
                    await currenSequence.AsyncWaitForCompletion();
                }
                finally
                {
                    currenSequence?.Kill(); // Остановка анимации при отмене
                }
            }
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            #endregion

            #region 2. Финальная фиксация направления 

            currenSequence = DOTween.Sequence();
            currenSequence.Join(DOTween.To(() => currentDirection, x => currentDirection = x, shotDirection, 0.5f)
                .SetEase(Ease.InOutQuad)
                .OnUpdate(() => { aiSelectedPawn.UpdateLineVisuals(shotPower, currentDirection); })
                .OnComplete(() => { aiSelectedPawn.UpdateLineVisuals(shotPower, shotDirection); })
                .OnKill(() => currenSequence = null));

            try
            {
                if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();
                await currenSequence.AsyncWaitForCompletion();
            }
            finally
            {
                currenSequence?.Kill(); // Остановка анимации при отмене
            }
            #endregion
        }
          
        #endregion
    }
}