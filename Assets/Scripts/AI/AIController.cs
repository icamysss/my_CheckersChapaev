using System.Collections;
using System.Collections.Generic;
using Game;
using Sirenix.OdinInspector;
using UnityEngine;

public class AIController : MonoBehaviour
{
    [SerializeField] private float decisionDelay = 1.5f;
    [SerializeField] private float maxPredictionDistance = 3f;
    [SerializeField] private float neighborRadius = 1.0f;
    
    public List<Pawn> _aiCheckers = new();
    public List<Pawn> _enemyCheckers = new();
    
    private Pawn targetChecker;
   [ShowInInspector] private Pawn aiChecker;
    private ScoreCalculator _scoreCalculator;

    void Start()
    {
        _scoreCalculator = new ScoreCalculator(neighborRadius, maxPredictionDistance);
    }

    public void MakeMove()
    {
        StartCoroutine(AIMoveRoutine());
    }

    private IEnumerator AIMoveRoutine()
    {
        Debug.Log("AI Move routine");
        yield return new WaitForSeconds(decisionDelay);
        
        aiChecker = SelectBestChecker();
        Debug.Log(aiChecker.name);
        if (aiChecker == null) yield break;
        
        var direction = CalculateAimDirection(aiChecker);
        Debug.Log(direction);
        ApplyAIShot(aiChecker, direction);
    }

    private Pawn SelectBestChecker()
    {
        Pawn bestChecker = null;
        float maxScore = float.MinValue;

        foreach (var checker in _aiCheckers)
        {
            if (checker == null) continue;
            
            float score = _scoreCalculator.Calculate(checker.transform.position, 
                _aiCheckers, 
                _enemyCheckers);
            
            if (score > maxScore)
            {
                maxScore = score;
                bestChecker = checker;
            }
        }
        return bestChecker;
    }

    private Vector3 CalculateAimDirection(Pawn selectedChecker)
    {
        if (_enemyCheckers.Count == 0)
        {
            Debug.LogWarning("No enemies found!");
            return Vector3.forward; // Направление по умолчанию
        }

        Vector3 targetPosition = _scoreCalculator.FindOptimalTarget(
            selectedChecker.transform.position, 
            _enemyCheckers);
    
        // Добавляем проверку на валидность цели
        if (Vector3.Distance(targetPosition, selectedChecker.transform.position) < 0.1f)
        {
            Debug.Log("Fallback target selection");
            targetPosition = FindFallbackTarget(selectedChecker.transform.position);
        }

        Vector3 direction = (targetPosition - selectedChecker.transform.position).normalized;
        Debug.DrawRay(selectedChecker.transform.position, direction * 2, Color.green, 2f);
    
        return direction;
    }
    private Vector3 FindFallbackTarget(Vector3 shooterPosition)
    {
        // Резервная логика: выбираем случайную цель
        if (_enemyCheckers.Count == 0) return shooterPosition + Vector3.forward;
    
        int randomIndex = Random.Range(0, _enemyCheckers.Count);
        return _enemyCheckers[randomIndex].transform.position;
    }

    private void ApplyAIShot(Pawn checker, Vector3 direction)
    {
        float distance = Vector3.Distance(checker.transform.position, 
            _scoreCalculator.LastCalculatedTarget);
        float forceMultiplier = Mathf.Clamp01(distance / maxPredictionDistance);
        float force = Mathf.Lerp(checker.minForce, checker.maxForce, forceMultiplier);
        
        checker.ApplyForce(direction * force);
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

    public float Calculate(Vector3 checkerPosition, List<Pawn> aiCheckers, List<Pawn> enemyCheckers)
    {
        float score = 0f;
        
        // Штраф за скученность своих шашек
        int friendlyCount = Physics.OverlapSphereNonAlloc(checkerPosition, _neighborRadius, 
            new Collider[10], LayerMask.GetMask("Friendly"));
        score -= friendlyCount * 2f;

        // Бонус за близость вражеских шашек
        int enemyCount = Physics.OverlapSphereNonAlloc(checkerPosition, _neighborRadius, 
            new Collider[10], LayerMask.GetMask("Enemy"));
        score += enemyCount * 3f;

        // Поиск лучшей цели
        Vector3 target = FindOptimalTarget(checkerPosition, enemyCheckers);
        float distance = Vector3.Distance(checkerPosition, target);
        
        // Бонус за близость к цели
        score += 10f / (distance + 0.1f);

        return score;
    }

    public Vector3 FindOptimalTarget(Vector3 shooterPosition, List<Pawn> enemies)
    {
        Vector3 bestTarget = shooterPosition;
        int maxHitCount = 0;

        foreach (var enemy in enemies)
        {
            if (enemy == null) continue;
            
            Vector3 direction = (enemy.transform.position - shooterPosition).normalized;
            int hits = PredictHits(shooterPosition, direction, enemies);
            
            if (hits > maxHitCount)
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
            LayerMask.GetMask("Enemy", "Obstacle"));

        Debug.DrawRay(origin, direction * _maxPredictionDistance, Color.yellow, 1f);

        foreach (var hit in hits)
        {
            if (hit.collider == null) continue;

            if (hit.collider.CompareTag("Enemy"))
            {
                hitCount++;
                Debug.Log($"Hit enemy: {hit.collider.name}");
            }
            else if (hit.collider.CompareTag("Obstacle"))
            {
                Debug.Log($"Hit obstacle: {hit.collider.name}");
                break;
            }
        }
        return hitCount;
    }
}