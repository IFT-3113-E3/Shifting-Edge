using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalCollision : MonoBehaviour
{
    [Tooltip("Le nom de la scène à charger")]
    public string sceneToLoad;

    [Tooltip("Vérifie si seulement le joueur peut déclencher le changement de scène")]
    public bool playerOnly = true;

    [Tooltip("Le tag de l'objet joueur (si playerOnly est vrai)")]
    public string playerTag = "Player";

    private void OnCollisionEnter(Collision collision)
    {
        // Vérifie si on accepte seulement le joueur ou tous les objets
        if (!playerOnly || (playerOnly && collision.gameObject.CompareTag(playerTag)))
        {
            // Charge la nouvelle scène
            SceneManager.LoadScene(sceneToLoad);
        }
    }

    // Si vous voulez utiliser des triggers au lieu de collisions solides
    private void OnTriggerEnter(Collider other)
    {
        if (!playerOnly || (playerOnly && other.CompareTag(playerTag)))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}