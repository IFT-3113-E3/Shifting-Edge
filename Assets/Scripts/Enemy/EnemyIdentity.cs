using UnityEngine;
using TMPro;

public class EnemyIdentity : MonoBehaviour
{
    private EnemyData data;
    
    [SerializeField]
    private TextMeshProUGUI nameText; // Optionnel, si vous voulez afficher le nom au-dessus
    
    public void SetEnemyData(EnemyData enemyData)
    {
        data = enemyData;
        
        // Si vous avez un TextMeshPro attaché, mettre à jour le nom
        if (nameText != null)
        {
            nameText.text = data.displayName;
        }
    }
    
    public string GetEnemyName()
    {
        return data != null ? data.displayName : "Unknown Enemy";
    }
    
    public string GetEnemyId()
    {
        return data != null ? data.enemyId : "";
    }
}