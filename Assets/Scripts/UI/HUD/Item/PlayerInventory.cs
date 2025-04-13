using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }
    
    [SerializeField] private int initialMana = 5; // Valeur configurable dans l'inspecteur
    public int SkillTreeMana { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            SkillTreeMana = initialMana; // Initialisation du mana ici
        }
    }

    public bool TrySpendMana(int amount)
    {
        if (SkillTreeMana >= amount)
        {
            SkillTreeMana -= amount;
            return true;
        }
        return false;
    }

    public void AddMana(int amount)
    {
        SkillTreeMana += amount;
    }
}