using UnityEngine;

public class SkillTreeManager : MonoBehaviour
{
    public static SkillTreeManager Instance { get; private set; }

    public SkillData[] allSkills;
    private PlayerInventory playerInventory;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        ResetAllSkills();
    }

    public void ResetAllSkills()
    {
        foreach (var skill in allSkills)
        {
            skill.isUnlocked = false;
        }
        Debug.Log("Toutes les compétences ont été réinitialisées");
    }


    void Start()
    {
        playerInventory = FindObjectOfType<PlayerInventory>();
    }

    public bool TryUnlockSkill(SkillData skill)
    {
        if (CanUnlock(skill) && PlayerInventory.Instance.TrySpendMana(skill.manaCost))
        {
            skill.isUnlocked = true;
            Debug.Log($"Compétence débloquée. Mana restant: {PlayerInventory.Instance.SkillTreeMana}");
            return true;
        }
        return false;
    }

    public bool CanUnlock(SkillData skill)
    {
        if (skill.isUnlocked) return false;
        
        foreach (var prereq in skill.prerequisites)
        {
            if (!prereq.isUnlocked) return false;
        }
        return true;
    }
}