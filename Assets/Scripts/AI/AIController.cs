using System.Collections;
using System.Collections.Generic;
using Core;
using Sirenix.OdinInspector;
using UnityEngine;

public class AIController : MonoBehaviour
{
    [SerializeField] private float decisionDelay = 1.5f;
    [SerializeField] private float maxPredictionDistance = 3f;
    [SerializeField] private float neighborRadius = 1.0f;
    
    [Header("Score Weights")]
    [SerializeField] private float friendlyGroupWeight = 0.7f;
    [SerializeField] private float enemyProximityWeight = 1.2f;
    [SerializeField] private float lineOfFireWeight = 2.0f;
    [SerializeField] private float boardCenterWeight = 0.5f;
    [SerializeField] private float edgePenaltyWeight = 0.3f;

    public List<Pawn> _aiPawns = new();
    public List<Pawn> _enemyPawns = new();

    [ShowInInspector, ReadOnly] private Pawn aiSelectedPawn;
    private ScoreCalculator _scoreCalculator;
    private Board _board;
    private Game _game;

    public void Initialize(Board board, Game game)
    {
        _scoreCalculator = new ScoreCalculator(neighborRadius, maxPredictionDistance);
        _board = board;
        _game = game;
    }

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

    private void RefreshPawnLists(PawnColor pawnColor)
    {
        _aiPawns = _board.GetPawnsOnBoard(pawnColor);
        _enemyPawns = _board.GetPawnsOnBoard(pawnColor == PawnColor.Black ? PawnColor.White : PawnColor.Black);
    }

    private IEnumerator AIMoveRoutine()
    {
        yield return new WaitForSeconds(decisionDelay);
        
        aiSelectedPawn = SelectOptimalPawn();
        if (aiSelectedPawn == null) yield break;

        Vector3 shotDirection = CalculateOptimalDirection(aiSelectedPawn);
        ExecuteAIShot(aiSelectedPawn, shotDirection);
    }

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
                friendlyGroupWeight,
                enemyProximityWeight,
                lineOfFireWeight,
                boardCenterWeight,
                edgePenaltyWeight,
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

    private Vector3 GetFallbackTarget(Vector3 currentPosition)
    {
        if (_enemyPawns.Count > 0)
        {
            return _enemyPawns[Random.Range(0, _enemyPawns.Count)].transform.position;
        }
        return currentPosition + Vector3.forward * 2f;
    }

    private Vector3 GetFallbackDirection(Transform pawnTransform)
    {
        return pawnTransform.forward + new Vector3(
            Random.Range(-0.3f, 0.3f),
            0,
            Random.Range(0.5f, 1f)
        ).normalized;
    }

    private void ExecuteAIShot(Pawn pawn, Vector3 direction)
    {
        float distance = Vector3.Distance(pawn.transform.position, _scoreCalculator.LastCalculatedTarget);
        float forceMultiplier = Mathf.Clamp01(distance / maxPredictionDistance);
        float force = Mathf.Lerp(pawn.minForce, pawn.maxForce, forceMultiplier);

        pawn.ApplyForce(direction * force);
        Debug.Log($"AI shot: {pawn.name} with force {force:F1} in direction {direction}");
    }
}

public class ScoreCalculator
{
    private readonly float _neighborRadius;
    private readonly float _maxPredictionDistance;
    public Vector3 LastCalculatedTarget { get; private set; }

    public ScoreCalculator(float neighborRadius, float maxPredictionDistance)
    {
        _neighborRadius = neighborRadius;
        _maxPredictionDistance = maxPredictionDistance;
    }

    public float Calculate(
        Vector3 checkerPosition,
        List<Pawn> friendlyPawns,
        List<Pawn> enemyPawns,
        float friendlyWeight,
        float enemyWeight,
        float lineOfFireWeight,
        float centerWeight,
        float edgePenalty,
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
        score += friendlyCount * friendlyWeight;

        // 2. Близость к врагам
        int enemyCount = Physics.OverlapSphereNonAlloc(
            checkerPosition,
            _neighborRadius,
            new Collider[10],
            LayerMask.GetMask("Enemy")
        );
        score += enemyCount * enemyWeight;

        // 3. Линия огня
        Vector3 target = FindOptimalTarget(checkerPosition, enemyPawns);
        Vector3 fireDirection = (target - checkerPosition).normalized;
        int potentialHits = PredictHits(checkerPosition, fireDirection, enemyPawns);
        score += potentialHits * lineOfFireWeight;

        // 4. Позиционирование
        float distanceFromCenter = Vector3.Distance(checkerPosition, Vector3.zero);
        score += (1 - Mathf.Clamp01(distanceFromCenter / (boardSize * 0.5f))) * centerWeight;

        // 5. Штраф за края
        float edgeFactor = Mathf.Clamp01(distanceFromCenter / (boardSize * 0.5f));
        score -= edgeFactor * edgePenalty;

        return score;
    }

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

    private int PredictHits(Vector3 origin, Vector3 direction, List<Pawn> enemies)
    {
        int hitCount = 0;
        RaycastHit[] hits = Physics.RaycastAll(
            origin,
            direction,
            _maxPredictionDistance,
            LayerMask.GetMask("Enemy", "Obstacle")
        );

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.CompareTag("Obstacle")) break;
            if (hit.collider.CompareTag("Enemy")) hitCount++;
        }

        return hitCount;
    }
}