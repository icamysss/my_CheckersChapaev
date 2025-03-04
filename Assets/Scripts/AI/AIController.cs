using System.Collections;
using System.Collections.Generic;
using Core;
using Sirenix.OdinInspector;
using UnityEngine;
using DG.Tweening;

public class AIController : MonoBehaviour
{
    #region Inspector Variables

    [Header("Timing Settings")]
    [SerializeField, Tooltip("Минимальное время принятия решения (сек)")]
    private float minDecisionDelay = 1f;
    [SerializeField, Tooltip("Максимальное время принятия решения (сек)")]
    private float maxDecisionDelay = 3f;
    [SerializeField, Tooltip("Время имитации прицеливания (сек)")]
    private float aimingTime = 2f;

    [Header("Score Calculation Settings")]
    [SerializeField, Tooltip("Радиус поиска соседних шашек")]
    private float neighborRadius = 1.0f;

    [Header("Score Weights")]
    [SerializeField, Tooltip("Вес группировки своих шашек")]
    private float friendlyGroupWeight = 0.7f;
    [SerializeField, Tooltip("Вес близости к вражеским шашкам")]
    private float enemyProximityWeight = 1.2f;
    [SerializeField, Tooltip("Вес линии огня (потенциальных попаданий)")]
    private float lineOfFireWeight = 2.0f;
    [SerializeField, Tooltip("Вес позиции ближе к центру доски")]
    private float boardCenterWeight = 0.5f;
    [SerializeField, Tooltip("Штраф за близость к краям доски")]
    private float edgePenaltyWeight = 0.3f;

    #endregion

    #region Private Variables

    public List<Pawn> _aiPawns = new();
    public List<Pawn> _enemyPawns = new();

    [ShowInInspector, ReadOnly]
    private Pawn aiSelectedPawn;
    private ScoreCalculator _scoreCalculator;
    private Board _board;
    private CameraController _cameraController;

    #endregion

    #region Initialization

    /// <summary>
    /// Инициализация контроллера ИИ с доской и камерой
    /// </summary>
    public void Initialize(Board board)
    {
        _board = board;
        _cameraController = FindFirstObjectByType<CameraController>();
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
    public void MakeMove(PawnColor pawnColor)
    {
        if (pawnColor == PawnColor.None)
        {
            Debug.LogError($"Invalid pawn color: {pawnColor}");
            return;
        }

        RefreshPawnLists(pawnColor);
        StartCoroutine(AIMoveRoutine());
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Обновление списков шашек ИИ и врагов
    /// </summary>
    private void RefreshPawnLists(PawnColor pawnColor)
    {
        _aiPawns = _board.GetPawnsOnBoard(pawnColor);
        _enemyPawns = _board.GetPawnsOnBoard(pawnColor == PawnColor.Black ? PawnColor.White : PawnColor.Black);
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
        Vector3 shotDirection = CalculateOptimalDirection(aiSelectedPawn);
        float force = CalculateForce(aiSelectedPawn, shotDirection);
        aiSelectedPawn.ApplyForce(shotDirection * force);
    }

    /// <summary>
    /// Выбор оптимальной шашки на основе скоринга
    /// </summary>
    private Pawn SelectOptimalPawn()
    {
        Pawn bestPawn = null;
        float maxScore = float.MinValue;

        foreach (Pawn pawn in _aiPawns)
        {
            if (pawn == null) continue;

            float score = _scoreCalculator.Calculate(
                pawn.transform.position,
                _aiPawns,
                _enemyPawns,
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
    float elapsedTime = 0f;
    Vector3 optimalDirection = CalculateOptimalDirection(aiSelectedPawn); // Оптимальное направление
    Vector3 currentDirection = optimalDirection + Random.insideUnitSphere * 0.5f; // Начальное случайное отклонение
    currentDirection.y = 0; // Ограничиваем движение по горизонтали
    currentDirection.Normalize();

    // Настройки колебаний
    int oscillationCount = 3; // Количество небольших корректировок
    float oscillationDuration = aimingTime / (oscillationCount + 1); // Время на каждую фазу

    // Этап 1: Колебания (имитация неуверенности или корректировки)
    for (int i = 0; i < oscillationCount; i++)
    {
        Vector3 targetDirection = optimalDirection + Random.insideUnitSphere * 0.2f; // Небольшое отклонение от цели
        targetDirection.y = 0;
        targetDirection.Normalize();

        // Плавный переход к новому направлению
        yield return DOTween.To(() => currentDirection, x => currentDirection = x, targetDirection, oscillationDuration)
            .SetEase(Ease.InOutQuad) // Плавное ускорение и замедление
            .OnUpdate(() =>
            {
                float force = CalculateForce(aiSelectedPawn, currentDirection);
                aiSelectedPawn.UpdateLineVisuals(force, currentDirection); // Обновляем визуализацию
            })
            .WaitForCompletion();
    }

    // Этап 2: Фиксация на оптимальном направлении
    yield return DOTween.To(() => currentDirection, x => currentDirection = x, optimalDirection, oscillationDuration)
        .SetEase(Ease.InOutQuad)
        .OnUpdate(() =>
        {
            float force = CalculateForce(aiSelectedPawn, currentDirection);
            aiSelectedPawn.UpdateLineVisuals(force, currentDirection); // Обновляем визуализацию
        })
        .WaitForCompletion();

    // Финальное обновление направления
    float finalForce = CalculateForce(aiSelectedPawn, optimalDirection);
    aiSelectedPawn.UpdateLineVisuals(finalForce, optimalDirection);
}

    /// <summary>
    /// Расчет оптимального направления выстрела
    /// </summary>
    private Vector3 CalculateOptimalDirection(Pawn selectedPawn)
    {
        if (_enemyPawns.Count == 0)
        {
            Debug.LogWarning("No enemies available for targeting");
            return GetFallbackDirection(selectedPawn.transform);
        }

        Vector3 targetPosition = _scoreCalculator.FindOptimalTarget(
            selectedPawn.transform.position,
            _enemyPawns
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
    private float CalculateForce(Pawn pawn, Vector3 direction)
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
        if (_enemyPawns.Count > 0)
        {
            return _enemyPawns[Random.Range(0, _enemyPawns.Count)].transform.position;
        }
        return currentPosition + Vector3.forward * 2f;
    }

    #endregion
}

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