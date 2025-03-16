using System.Collections.Generic;
using Core;
using UnityEngine;

namespace AI
{
    public class AICalculator
    {
        #region Properties

        public Vector3 CalculatedTarget { get; private set; }

        #endregion

        #region Private Variables
        
        private readonly AISettings aiSettings;
        private readonly float MaxPredictionDistance;
        private readonly Board board;

        private List<Pawn> friendlyPawns;
        private List<Pawn> enemyPawns;

        private int maxHitCount;

        #endregion

        #region Constructor

        /// <summary>
        /// Конструктор калькулятора очков
        /// </summary>
        public AICalculator(Player aiPlayer, Board board, List<Pawn> friendlyPawns, List<Pawn> enemyPawns )
        {
            MaxPredictionDistance = board.BoardSize; // Устанавливаем равным размеру доски
            aiSettings = aiPlayer.AISettings;
            this.board = board;
            this.friendlyPawns = friendlyPawns;
            this.enemyPawns = enemyPawns;
        }

        #endregion

        #region Public Methods
        
        /// <summary>
        /// Расчет силы выстрела на основе расстояния до цели
        /// </summary>
        public float CalculateForce(Pawn pawn)
        {
            if (pawn == null) return 0f;

            var distance = Vector3.Distance(pawn.transform.position, CalculatedTarget);
            var forceMultiplier = Mathf.Clamp01(distance / board.BoardSize);
            var force = Mathf.Lerp(pawn.minForce, pawn.maxForce, forceMultiplier);

            return maxHitCount > 1 ? pawn.maxForce * Random.Range(0.75f, 1f) : force;
        }

        /// <summary>
        /// Выбор оптимальной шашки на основе скоринга
        /// </summary>
        public Pawn SelectOptimalPawn()
        {
            Pawn bestPawn = null;
            var maxScore = float.MinValue;

            foreach (var pawn in friendlyPawns)
            {
                if (pawn == null) continue;

                var score = CalculateScore(pawn.transform.position);

                if (score > maxScore)
                {
                    maxScore = score;
                    bestPawn = pawn;
                }
            }
            return bestPawn;
        }
        
        /// <summary>
        /// Расчет оптимального направления выстрела
        /// </summary>
        public Vector3 CalculateOptimalDirection(Pawn selectedPawn)
        {
            if (enemyPawns.Count == 0)
            {
                Debug.LogWarning("No enemies available for targeting");
                return GetFallbackDirection(selectedPawn.transform);
            }

            enemyPawns.RemoveAll(p => p == null);
            var targetPosition = FindOptimalTarget(selectedPawn.transform.position);

            if (Vector3.Distance(targetPosition, selectedPawn.transform.position) < 0.1f)
            {
                targetPosition = GetFallbackTarget(selectedPawn.transform.position);
            }

            var direction = (targetPosition - selectedPawn.transform.position).normalized;
            Debug.DrawRay(selectedPawn.transform.position, direction * 2, Color.green, 2f);
            return direction;
        }

        #endregion

        #region Private Methods
        
          /// <summary>
        /// Расчет очков для позиции шашки
        /// </summary>
        private float CalculateScore(Vector3 checkerPosition)
        {
            
            var score = 0f;
            
            if (friendlyPawns == null || enemyPawns == null) 
            {
                Debug.LogError("Pawn lists are null!");
                return score;
            }

            #region 1. Группировка своих шашек

            var friendly = new Collider[16];
            var friendlyCount = 0;

            var numFound = Physics.OverlapSphereNonAlloc(
                checkerPosition,
                aiSettings.NeighborRadius,
                friendly,
                LayerMask.GetMask("Pawn")
                );

            // Обрабатываем только валидные коллайдеры
            for (var i = 0; i < numFound; i++)
            {
                var col = friendly[i];
                if (col == null) continue;

                var fp = col.GetComponent<Pawn>();
                if (fp != null && friendlyPawns.Contains(fp))
                {
                    friendlyCount++;
                }
            }

            score += friendlyCount * aiSettings.FriendlyGroupWeight;

            #endregion

            #region 2. Близость к врагам
            
            var enemy = new Collider[16];
            var enemyCount = 0;
            Physics.OverlapSphereNonAlloc(checkerPosition, aiSettings.NeighborRadius, enemy, LayerMask.GetMask("Pawn"));

            foreach (var e in enemy)
            {
                if (e == null) continue;
                var enPawn = e.GetComponent<Pawn>();
                if (enPawn != null && enemyPawns.Contains(enPawn)) enemyCount++;
            }
            
            score += enemyCount * aiSettings.EnemyProximityWeight;

            #endregion

            #region 3. Линия огня

            var target = FindOptimalTarget(checkerPosition);
            var fireDirection = (target - checkerPosition).normalized;
            var potentialHits = PredictHits(checkerPosition, fireDirection, out var friendlyInLine);
            
            score += potentialHits * aiSettings.LineOfFireWeight;
            score -= friendlyInLine * aiSettings.LineOfFriendlyFireWeight;
            
            #endregion
            
            #region 4. Позиционирование ближе к центру
            
            var distanceFromCenter = Vector3.Distance(checkerPosition, board.CenterPosition);
            score += (1 - Mathf.Clamp01(distanceFromCenter / (board.BoardSize * 0.5f))) * aiSettings.BoardCenterWeight;

            #endregion
            
            #region 5. Штраф за края
            
            var edgeFactor = Mathf.Clamp01(distanceFromCenter / (board.BoardSize * 0.5f));
            score -= edgeFactor * aiSettings.EdgePenaltyWeight;
            
            #endregion
            
            return score;
            
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

        /// <summary>
        /// Поиск оптимальной цели для выстрела
        /// </summary>
        private Vector3 FindOptimalTarget(Vector3 shooterPosition)
        {
            var bestTarget = shooterPosition;
            maxHitCount = 0;
            
            foreach (var enemy in enemyPawns)
            {
                if (enemy == null) continue;
                var direction = (enemy.transform.position - shooterPosition).normalized;
                var hits = PredictHits(shooterPosition, direction, out var friendlyCount);
                if (friendlyCount > 2) hits -= friendlyCount;
                if (hits <= maxHitCount && 
                    (hits != maxHitCount ||
                     !(Vector3.Distance(shooterPosition, enemy.transform.position) >
                       Vector3.Distance(shooterPosition, bestTarget)))) continue;
                maxHitCount = hits;
                bestTarget = enemy.transform.position;
                CalculatedTarget = bestTarget;
            }
            
            return bestTarget;
        }
        
        /// <summary>
        /// Предсказание количества попаданий в заданном направлении
        /// </summary>
        private int PredictHits(Vector3 origin, Vector3 direction, out int frendly)
        {
            frendly = 0;
            var hitCount = 0;
            var hits = Physics.RaycastAll(
                origin,
                direction,
                MaxPredictionDistance,
                LayerMask.GetMask("Pawn")
                );

            foreach (RaycastHit hit in hits)
            {
                var pawn = hit.collider.GetComponent<Pawn>();
                if (pawn != null && enemyPawns.Contains(pawn))
                {
                    hitCount++;
                }
                else if (friendlyPawns.Contains(pawn))
                {
                    frendly++;
                }
            }

            return hitCount;
        }

        #endregion
    }
}