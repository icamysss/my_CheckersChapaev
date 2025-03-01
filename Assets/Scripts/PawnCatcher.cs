using UnityEngine;

public class PawnCatcher : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Pawn")) return;
        
       // collision.gameObject.SetActive(false);
    }
}