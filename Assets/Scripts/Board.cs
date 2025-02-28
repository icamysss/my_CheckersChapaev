using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class Board : MonoBehaviour
{
    [ShowInInspector, ReadOnly] private List<Pawn> pawns = new();
    [SerializeField] private Vector3 A1Coordinates;
    [SerializeField] private float cellSize;
    [SerializeField] private GameObject pawnPrefab;
    
    public List<Pawn> GetPawns()
    {
       return pawns;
    }

    public void Reset()
    {
        throw new NotImplementedException();
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
    public void SpawnPawns()
    {
        for (var i = 0; i < 8; i++)
        {
            var spawnPos = new Vector3(A1Coordinates.x - cellSize * i, 0, A1Coordinates.z);
            var go = Instantiate(pawnPrefab, spawnPos, Quaternion.identity);
            var pawn = go.GetComponent<Pawn>();
            pawn.SetColor(PawnColor.White);
        }
        
        for (var i = 0; i < 8; i++)
        {
            var spawnPos = new Vector3(A1Coordinates.x - cellSize * i, 0, A1Coordinates.z - cellSize * 7);
            var go = Instantiate(pawnPrefab, spawnPos, Quaternion.identity);
            var pawn = go.GetComponent<Pawn>();
            pawn.SetColor(PawnColor.Black);
        }
    }

    [Button]
    public void DeletePawns()
    {
        var go = GameObject.FindGameObjectsWithTag("Pawn");
        foreach (var pawn in go)
        {
            DestroyImmediate(pawn);
        }
            
    }
}