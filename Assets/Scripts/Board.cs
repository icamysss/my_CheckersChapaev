using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// Важно что б объект на котором находится скрипт находился четко в центре доски! От этого расчитываются ячейки !
/// </summary>
public class Board : MonoBehaviour
{
    [ShowInInspector, ReadOnly] private List<Pawn> pawns = new();
    [SerializeField] private int boardSize = 8;
    [SerializeField] private float cellSize;
    [SerializeField] private Pawn pawnPrefab;

    public Vector3 CenterPosition => transform.position;  // середина доски
    
    public List<Pawn> GetPawns()
    {
       return pawns;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="x">Буквенная сторона А = 1, B = 2 и тд</param>
    /// <param name="y">Числовая сторона</param>
    /// <returns></returns>
    public Vector3 GetCellPosition(int x, int y)
    {
        var halfBoard = (boardSize - 1) * 0.5f;
        var posX = (x - halfBoard) * cellSize;
        var posZ = (y - halfBoard) * cellSize;
        return CenterPosition + new Vector3(posX, 0, posZ);
    }
    
    public Pawn SpawnPawn(int x, int y, PawnColor color)
    {
        var spawnPosition = GetCellPosition(x, y);
        var newPawn = Instantiate(pawnPrefab, spawnPosition, Quaternion.identity, transform);
        
        if (newPawn == null) return null;
        newPawn.PawnColor = color;
        if (!pawns.Contains(newPawn)) pawns.Add(newPawn);
        return newPawn;
    }
    
    public void Reset()
    {
        throw new NotImplementedException();
    }
    
    [Button]
    public void SetupStandardPosition()
    {
        // Spawn white pawns (rows 0-2)
        for (int y = 0; y < 1; y++)
        {
            for (int x = 0; x < boardSize; x++)
            {
                SpawnPawn(x, y, PawnColor.White);
            }
        }

        // Spawn black pawns (last 3 rows)
        for (int y = boardSize - 1; y < boardSize; y++)
        {
            for (int x = 0; x < boardSize; x++)
            {
                SpawnPawn(x, y, PawnColor.Black);
            }
        }
    }
    
    private void OnCollisionEnter(Collision other)
    {
        if (!other.gameObject.CompareTag("Pawn")) return;
        
        var pawn = other.gameObject.GetComponent<Pawn>();
        if (pawn == null) return;
        if (!pawns.Contains(pawn)) pawns.Add(pawn);
    }

    private void OnCollisionExit(Collision other)
    {
        if (!other.gameObject.CompareTag("Pawn")) return;
        var pawn = other.gameObject.GetComponent<Pawn>();
        if (pawn == null) return;
        pawns.Remove(pawn);
    }
    
    [Button]
    public void DeleteAllPawns()
    {
        var go = GameObject.FindGameObjectsWithTag("Pawn");
        foreach (var pawn in go)
        {
            DestroyImmediate(pawn);
            pawns.Clear();
            pawns = new List<Pawn>();
        }
            
    }
}