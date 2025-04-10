using UnityEngine;
using Status;

public class EnemyXP : MonoBehaviour
{
    [SerializeField] private int xpReward = 10;
    [SerializeField] private GameObject xpFloatingTextPrefab; // Prefab pour afficher l'XP gagnée
    
    private EntityStatus entityStatus;
    private EnemyIdentity enemyIdentity;
    private bool xpAwarded = false;
    
    private void Awake()
    {
        entityStatus = GetComponent<EntityStatus>();
        enemyIdentity = GetComponent<EnemyIdentity>();
        
        if (entityStatus == null)
        {
            Debug.LogError("EnemyXP requires an EntityStatus component on the same GameObject.", this);
            enabled = false;
            return;
        }
        
        // S'abonner à l'événement de mort
        entityStatus.OnDeath += HandleEnemyDeath;
    }
    
    private void Start()
    {
        // Récupérer la valeur d'XP depuis l'EnemyIdentity si disponible
        if (enemyIdentity != null && !string.IsNullOrEmpty(enemyIdentity.GetEnemyId()))
        {
            EnemyData enemyData = EnemyManager.Instance.GetEnemyData(enemyIdentity.GetEnemyId());
            if (enemyData != null)
            {
                xpReward = enemyData.xpReward;
            }
        }
    }
    
    private void HandleEnemyDeath(DamageRequest damageRequest)
    {
        if (xpAwarded) return; // Éviter de donner l'XP plusieurs fois
        GameObject killer = damageRequest.source; // Le joueur qui a tué l'ennemi
        
        // Vérifier si c'est le joueur qui a tué l'ennemi
        if (killer != null && killer.CompareTag("Player"))
        {
            // Donner l'XP au joueur
            PlayerXP playerXP = killer.GetComponent<PlayerXP>();
            if (playerXP != null)
            {
                playerXP.AddXP(xpReward);
                ShowXPGained(xpReward);
                xpAwarded = true;
                
                Debug.Log($"Player awarded {xpReward} XP for defeating {gameObject.name}");
            }
        }
    }
    
    private void ShowXPGained(int amount)
    {
        // Afficher un texte flottant montrant l'XP gagnée (facultatif)
        if (xpFloatingTextPrefab != null)
        {
            GameObject floatingText = Instantiate(xpFloatingTextPrefab, transform.position + Vector3.up, Quaternion.identity);
            
            // Si le prefab a un script TextMesh/TextMeshPro, définir le texte
            TextMesh textMesh = floatingText.GetComponentInChildren<TextMesh>();
            if (textMesh != null)
            {
                textMesh.text = $"+{amount} XP";
            }
            
            // Si vous utilisez TextMeshPro à la place
            TMPro.TextMeshProUGUI tmpText = floatingText.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (tmpText != null)
            {
                tmpText.text = $"+{amount} XP";
            }
            
            // Détruire le texte après quelques secondes
            Destroy(floatingText, 2f);
        }
    }
    
    private void OnDestroy()
    {
        // Se désabonner de l'événement
        if (entityStatus != null)
        {
            entityStatus.OnDeath -= HandleEnemyDeath;
        }
    }
}