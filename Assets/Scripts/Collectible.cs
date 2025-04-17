using UnityEngine;

public class Collectible : MonoBehaviour
{
    public static int totalCollected = 0;

    private bool isCollected = false;

    void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;

        if (other.CompareTag("Player"))
        {
            isCollected = true;
            totalCollected++;
            Destroy(gameObject);
        }
    }
}
