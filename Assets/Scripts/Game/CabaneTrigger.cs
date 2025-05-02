using UnityEngine;

public class CabaneTrigger : MonoBehaviour
{
    [Header("Environnement")]
    public GameObject interiorRoot;
    public GameObject exteriorRoot;
    public Transform insideSpawnPoint;
    public Transform outsideSpawnPoint;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip doorClip;

    private Transform player;
    private bool isPlayerInside = false;
    private bool isPlayerInZone = false;

    void Update()
    {
        if (isPlayerInZone && Input.GetKeyDown(KeyCode.E))
        {
            ToggleCabane();
        }
    }

    void ToggleCabane()
    {
        isPlayerInside = !isPlayerInside;

        interiorRoot.SetActive(isPlayerInside);
        exteriorRoot.SetActive(!isPlayerInside);

        if (player != null)
        {
            Transform target = isPlayerInside ? insideSpawnPoint : outsideSpawnPoint;
            player.position = target.position;
        }

        if (audioSource != null)
        {
            audioSource.PlayOneShot(doorClip);
        }

        InteractionUI.Instance.ShowText(isPlayerInside ? "Appuyez sur E pour sortir" : "Appuyez sur E pour entrer");
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.transform;
            isPlayerInZone = true;
            InteractionUI.Instance.ShowText(isPlayerInside ? "Appuyez sur E pour sortir" : "Appuyez sur E pour entrer");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = false;
            InteractionUI.Instance.HideText();
        }
    }
}
