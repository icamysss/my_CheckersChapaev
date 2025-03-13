using System.Collections;
using Core;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class PawnCatcher : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Pawn")) return;
        DisablePawn(collision.gameObject).Forget();
    }

    private async UniTask DisablePawn(GameObject pawn)
    {
        await UniTask.Delay(3000);
        if (pawn == null) return;
        
        var rb = pawn.GetComponent<Rigidbody>();
        if (rb)  rb.isKinematic = true;
        
        var p = pawn.GetComponent<Pawn>();
        if (p)  p.enabled = false;
    }
}