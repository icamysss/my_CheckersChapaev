using System;
using System.Collections;
using System.Collections.Generic;
using Core;
using Cysharp.Threading.Tasks;
using Services;
using UnityEngine;

public class PawnCatcher : MonoBehaviour
{
    private Game currentGame;
    private List<GameObject> pawnsGo = new();
    private const int TIME_TO_DESTROY = 3000;
    private const int TIME_TO_MUTE = 1000;
    
    private void OnEnable()
    {
        ServiceLocator.OnAllServicesRegistered += OnAllRegistered;
    }

    private void OnAllRegistered()
    {
        ServiceLocator.OnAllServicesRegistered -= OnAllRegistered;
        currentGame = ServiceLocator.Get<IGameManager>().CurrentGame;
        currentGame.OnStart += () => { pawnsGo.Clear(); };
    }

    private void OnDisable()
    {
      currentGame.OnStart -= () => { pawnsGo.Clear(); };
    }

   
    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Pawn")) return;
        if (pawnsGo.Contains(collision.gameObject)) return;
        
        pawnsGo.Add(collision.gameObject);
        DisablePawn(collision.gameObject).Forget();
    }

    private async UniTask DisablePawn(GameObject pawn)
    {
        if (pawn == null) return;
        await UniTask.Delay(TIME_TO_MUTE);
        Destroy(pawn.GetComponent<Pawn>());
        
        await UniTask.Delay(TIME_TO_DESTROY);
        Destroy(pawn);
        
        
        // var rb = pawn.GetComponent<Rigidbody>();
        // if (rb)  rb.isKinematic = true;
        //
        // var p = pawn.GetComponent<Pawn>();
        // if (p)  p.enabled = false;
    }
}