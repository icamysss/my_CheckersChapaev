using System.Collections.Generic;
using Core;
using UnityEngine;

namespace AI
{
    public class ScoreCalculator
    {
        #region Properties

        public Vector3 CalculatedTarget { get; private set; }

        #endregion

        #region Private Variables
        
        private readonly AISettings aiSettings;
        private readonly float MaxPredictionDistance;
        private readonly Board board;

        #endregion

        #region Constructor

        /// <summary>
        /// Конструктор калькулятора очков
        /// </summary>
        public ScoreCalculator(Player aiPlayer, Board board)
        {
            MaxPredictionDistance = board.BoardSize; // Устанавливаем равным размеру доски
            aiSettings = aiPlayer.AISettings;
            this.board = board;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Расчет очков для позиции шашки
        /// </summary>
        public float Calculate(Vector3 checkerPosition, List<Pawn> friendlyPawns, List<Pawn> enemyPawns)
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

            var target = FindOptimalTarget(checkerPosition, enemyPawns);
            var fireDirection = (target - checkerPosition).normalized;
            var potentialHits = PredictHits(checkerPosition, fireDirection, enemyPawns);
            
            score += potentialHits * aiSettings.LineOfFireWeight;

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
        /// Поиск оптимальной цели для выстрела
        /// </summary>
        public Vector3 FindOptimalTarget(Vector3 shooterPosition, List<Pawn> enemies)
        {
            var bestTarget = shooterPosition;
            var maxHitCount = 0;

            foreach (var enemy in enemies)
            {
                if (enemy == null) continue;

                var direction = (enemy.transform.position - shooterPosition).normalized;
                var hits = PredictHits(shooterPosition, direction, enemies);

                if (hits <= maxHitCount && 
                    (hits != maxHitCount ||
                     !(Vector3.Distance(shooterPosition, enemy.transform.position) <
                       Vector3.Distance(shooterPosition, bestTarget)))) continue;
                maxHitCount = hits;
                bestTarget = enemy.transform.position;
                CalculatedTarget = bestTarget;
            }

            return bestTarget;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Предсказание количества попаданий в заданном направлении
        /// </summary>
        private int PredictHits(Vector3 origin, Vector3 direction, List<Pawn> enemies)
        {
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
                if (pawn != null && enemies.Contains(pawn))
                {
                    hitCount++;
                }
            }

            return hitCount;
        }

        #endregion
    }
}