using System.Collections.Generic;
using Core;
using UnityEngine;

namespace AI
{
    public class ScoreCalculator
    {
        #region Properties

        public float FriendlyGroupWeight { get; set; }
        public float EnemyProximityWeight { get; set; }
        public float LineOfFireWeight { get; set; }
        public float BoardCenterWeight { get; set; }
        public float EdgePenaltyWeight { get; set; }
        public float MaxPredictionDistance { get; private set; }
        public Vector3 LastCalculatedTarget { get; private set; }

        #endregion

        #region Private Variables

        private readonly float _neighborRadius;

        #endregion

        #region Constructor

        /// <summary>
        /// Конструктор калькулятора очков
        /// </summary>
        public ScoreCalculator(float neighborRadius, float boardSize)
        {
            _neighborRadius = neighborRadius;
            MaxPredictionDistance = boardSize; // Устанавливаем равным размеру доски
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Расчет очков для позиции шашки
        /// </summary>
        public float Calculate(
            Vector3 checkerPosition,
            List<Pawn> friendlyPawns,
            List<Pawn> enemyPawns,
            float boardSize)
        {
            float score = 0f;

            // 1. Группировка своих шашек
            int friendlyCount = Physics.OverlapSphereNonAlloc(
                checkerPosition,
                _neighborRadius,
                new Collider[10],
                LayerMask.GetMask("Friendly")
                );
            score += friendlyCount * FriendlyGroupWeight;

            // 2. Близость к врагам
            int enemyCount = Physics.OverlapSphereNonAlloc(
                checkerPosition,
                _neighborRadius,
                new Collider[10],
                LayerMask.GetMask("Enemy")
                );
            score += enemyCount * EnemyProximityWeight;

            // 3. Линия огня
            Vector3 target = FindOptimalTarget(checkerPosition, enemyPawns);
            Vector3 fireDirection = (target - checkerPosition).normalized;
            int potentialHits = PredictHits(checkerPosition, fireDirection, enemyPawns);
            score += potentialHits * LineOfFireWeight;

            // 4. Позиционирование ближе к центру
            float distanceFromCenter = Vector3.Distance(checkerPosition, Vector3.zero);
            score += (1 - Mathf.Clamp01(distanceFromCenter / (boardSize * 0.5f))) * BoardCenterWeight;

            // 5. Штраф за края
            float edgeFactor = Mathf.Clamp01(distanceFromCenter / (boardSize * 0.5f));
            score -= edgeFactor * EdgePenaltyWeight;

            return score;
        }

        /// <summary>
        /// Поиск оптимальной цели для выстрела
        /// </summary>
        public Vector3 FindOptimalTarget(Vector3 shooterPosition, List<Pawn> enemies)
        {
            Vector3 bestTarget = shooterPosition;
            int maxHitCount = 0;

            foreach (Pawn enemy in enemies)
            {
                if (enemy == null) continue;

                Vector3 direction = (enemy.transform.position - shooterPosition).normalized;
                int hits = PredictHits(shooterPosition, direction, enemies);

                if (hits > maxHitCount ||
                    (hits == maxHitCount &&
                     Vector3.Distance(shooterPosition, enemy.transform.position) <
                     Vector3.Distance(shooterPosition, bestTarget)))
                {
                    maxHitCount = hits;
                    bestTarget = enemy.transform.position;
                    LastCalculatedTarget = bestTarget;
                }
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
            int hitCount = 0;
            RaycastHit[] hits = Physics.RaycastAll(
                origin,
                direction,
                MaxPredictionDistance,
                LayerMask.GetMask("Enemy", "Obstacle")
                );

            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.CompareTag("Obstacle")) break;
                if (hit.collider.CompareTag("Enemy")) hitCount++;
            }

            return hitCount;
        }

        #endregion
    }
}