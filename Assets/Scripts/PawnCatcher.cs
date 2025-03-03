using System.Collections;
using UnityEngine;

public class PawnCatcher : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Pawn")) return;
        
      StartCoroutine(DisablePawn(collision.gameObject));
    }

    private IEnumerator DisablePawn(GameObject pawn)
    {
        yield return new WaitForSeconds(3f);
        
        var rb = pawn.GetComponent<Rigidbody>();
        if (rb)  rb.isKinematic = true;
    }
}