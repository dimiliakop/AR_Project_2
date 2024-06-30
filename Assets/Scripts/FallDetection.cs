using UnityEngine;

public class FallDetection : MonoBehaviour
{
    public ModeManager modeManager;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlacedObject"))
        {
            Destroy(other.gameObject);
            modeManager.CheckForWin();
        }
    }
}
