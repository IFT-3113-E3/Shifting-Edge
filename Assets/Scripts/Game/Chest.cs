using UnityEngine;

public class Chest : MonoBehaviour
{
    public Animator animator;
    public ItemData itemToGive;
    public GameObject indicatorPrefab; // Prefab de l'indicateur (icône + texte)
    public Transform indicatorSpawnPoint;
    public float indicatorDuration = 2f;

    private bool _opened = false;

    void OnTriggerEnter(Collider other)
    {
        if (_opened) return;

        if (other.CompareTag("Player"))
        {
            _opened = true;
            StartCoroutine(OpenChest());
        }
    }

    private System.Collections.IEnumerator OpenChest()
    {
        animator.SetTrigger("Open");
        yield return new WaitForSeconds(1f); // attendre l'ouverture

        InventoryManager.Instance.AddItem(itemToGive);

        // Instancie un indicateur temporaire
        if (indicatorPrefab && indicatorSpawnPoint)
        {
            GameObject indicator = Instantiate(indicatorPrefab, indicatorSpawnPoint.position, Quaternion.identity);
            ChestItemIndicator popup = indicator.GetComponent<ChestItemIndicator>();
            popup?.Initialize(itemToGive, indicatorDuration);
        }

        yield return new WaitForSeconds(3f);
        animator.SetTrigger("Close");
    }
}
