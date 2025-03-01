using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.PlayerLoop;

/// <summary>
/// Класс, управляющий логикой игровой доски и размещением пешек
/// </summary>
/// <remarks>
/// Важно: Объект с этим скриптом должен быть точно отцентрирован в координатах доски
/// </remarks>
public class Board : MonoBehaviour
{
    #region Inspector Variables
    [Title("Board Settings")]
    [SerializeField, Range(4, 12), Tooltip("Размер доски в клетках")]
    private int boardSize = 8;
    
    [SerializeField, Min(0.1f), Tooltip("Размер одной клетки в юнитах")]
    private float cellSize = 0.9f;

    [SerializeField, Required, Tooltip("Префаб пешки для спавна")]
    private Pawn pawnPrefab;

   
    [ShowInInspector, ReadOnly, PropertyOrder(100), Tooltip("Список всех пешек на доске")]
    private List<Pawn> pawns = new();
  
    [field: Title("Debug")]
    [field: ShowInInspector, PropertyOrder(90)]
    [field: ReadOnly]
    [field: Tooltip("Нужна проверка, шашки вылетели с доски")]
    public bool NeedCheckPawnsOnBoard { private get; set; } = true;

    #endregion

    #region Public Properties
    /// <summary>
    /// Центральная позиция доски в мировых координатах
    /// </summary>
    public Vector3 CenterPosition => transform.position;
    
    #endregion

    #region Public Methods
    /// <summary>
    /// Получить список всех пешек на доске
    /// </summary>
    public List<Pawn> GetPawns() => new(pawns);

    /// <summary>
    /// Получить мировые координаты для указанной клетки
    /// </summary>
    /// <param name="column">Колонка (буквенная координата, A=1, B=2...)</param>
    /// <param name="row">Ряд (числовая координата)</param>
    public Vector3 GetCellPosition(int column, int row)
    {
        var halfBoard = (boardSize - 1) * 0.5f;
        return CenterPosition + new Vector3(
            (column - halfBoard) * cellSize,
            0,
            (row - halfBoard) * cellSize
        );
    }

    /// <summary>
    /// Создать пешку в указанной клетке
    /// </summary>
    /// <param name="column">Колонка (1-based)</param>
    /// <param name="row">Ряд (1-based)</param>
    /// <param name="color">Цвет пешки</param>
    /// <returns>Созданная пешка или null при ошибке</returns>
    public Pawn SpawnPawn(int column, int row, PawnColor color)
    {
        if (pawnPrefab == null)
        {
            Debug.LogError("Pawn prefab is not assigned!");
            return null;
        }

        var spawnPosition = GetCellPosition(column, row);
        var newPawn = Instantiate(pawnPrefab, spawnPosition, Quaternion.identity, transform);
        
        newPawn.PawnColor = color;
        if (!pawns.Contains(newPawn)) pawns.Add(newPawn);
        return newPawn;
    }

    [Button]
    public void CheckForPawnsOnBoard()
    {
        Vector3 halfExtents = new Vector3(boardSize, 1, boardSize);
        var targetTag = "Pawn";
        float maxDistance = boardSize * 0.5f;
        
        Vector3 center = CenterPosition;
        Vector3 direction = transform.forward;
        Quaternion orientation = transform.rotation;
        

        // Выполняем BoxCast и получаем все попадания
        RaycastHit[] hits = Physics.BoxCastAll(
            center, 
            halfExtents, 
            direction, 
            orientation, 
            maxDistance, layerMask: 1 << LayerMask.NameToLayer(targetTag)
            );
        
        pawns.Clear(); pawns = new List<Pawn>();
        
        // Добавляем объекты с нужным тегом
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.CompareTag(targetTag))
            {
                pawns.Add(hit.collider.gameObject.GetComponent<Pawn>());
            }
        }
    }
    
    #endregion

    #region Setup Methods
    [Button(ButtonSizes.Large), PropertyOrder(50)]
    [Tooltip("Установить стартовую позицию пешек")]
    public void SetupStandardPosition()
    {
        ClearBoard();
        SpawnPawnsForColor(PawnColor.White, 0, 1);
        SpawnPawnsForColor(PawnColor.Black, boardSize - 1, boardSize);
    }

    [Button(ButtonSizes.Medium), PropertyOrder(51)]
    [Tooltip("Полная очистка доски")]
    public void DeleteAllPawns()
    {
        ClearBoard();
    }

    /// <summary>
    /// Спавнит пешки для указанного цвета в диапазоне рядов
    /// </summary>
    private void SpawnPawnsForColor(PawnColor color, int startRow, int endRow)
    {
        for (int row = startRow; row < endRow; row++)
        {
            for (int column = 0; column < boardSize; column++)
            {
                SpawnPawn(column, row, color);
            }
        }
    }

    /// <summary>
    /// Полная очистка доски от пешек
    /// </summary>
    private void ClearBoard()
    {
        foreach (var pawn in pawns)
        {
            if (pawn != null) DestroyImmediate(pawn.gameObject);
        }
        pawns.Clear();
    }
    #endregion

    #region Unity Callbacks
    
    #endregion

    #region Unimplemented
    public void Reset()
    {
        /* Реализация может потребоваться для расширения функционала */
        throw new NotImplementedException();
    }
    #endregion
}