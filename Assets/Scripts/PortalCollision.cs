using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalCollision : MonoBehaviour
{
    [Tooltip("Le nom de la scène à charger")]
    public string exitId;

    [Tooltip("Vérifie si seulement le joueur peut déclencher le changement de scène")]
    public bool playerOnly = true;

    [Tooltip("Le tag de l'objet joueur (si playerOnly est vrai)")]
    public string playerTag = "Player";

    [Tooltip("Le nombre d'objets requis pour ouvrir le portail")]
    public CollectibleData[] requiredCollectibles = Array.Empty<CollectibleData>();

    [Tooltip("Si le portail est ouvert ou non")]
    private bool isOpen = false;

    private void OnCollisionEnter(Collision collision)
    {
        // Vérifie si on accepte seulement le joueur ou tous les objets
        if (isOpen && (!playerOnly || (playerOnly && collision.gameObject.CompareTag(playerTag))))
        {
            // Charge la nouvelle scène
            GameManager.Instance.TransitionTo(exitId);
        }
    }

    // Si vous voulez utiliser des triggers au lieu de collisions solides
    private void OnTriggerEnter(Collider other)
    {
        if (isOpen && (!playerOnly || (playerOnly && other.CompareTag(playerTag))))
        {
            GameManager.Instance.TransitionTo(exitId);
        }
    }

    private void OnEnable()
    {
        GameManager.Instance.GameSession.GameProgression.OnCollectibleCollected += OnCollectibleCollected;
        CheckCollectibles();
    }
    
    private void OnDisable()
    {
        GameManager.Instance.GameSession.GameProgression.OnCollectibleCollected -= OnCollectibleCollected;
    }
    
    private void OnCollectibleCollected(string collectible)
    {
        CheckCollectibles();
    }

    private void CheckCollectibles()
    {
        var progression = GameManager.Instance.GameSession.GameProgression;

        // check all required items against current progression
        foreach (var collectible in requiredCollectibles)
        {
            if (!progression.HasCollected(collectible.id))
            {
                isOpen = false;
                return;
            }
        }

        OpenPortal();
    }


    void OpenPortal()
    {
        isOpen = true;
        Debug.Log("Portail ouvert !");
    }
}
