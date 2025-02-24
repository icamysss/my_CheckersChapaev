using UnityEngine;

public class CheckerCatcher : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Checker"))
        {
            var selectedChecker = GameManager.Instance.SelectedChecker;
            var catchedChecker = other.gameObject.GetComponent<Checker>();
            
            if (selectedChecker != null && selectedChecker == catchedChecker) 
                GameManager.Instance.SelectedChecker = null;
            other.gameObject.SetActive(false);
        }
    }
}