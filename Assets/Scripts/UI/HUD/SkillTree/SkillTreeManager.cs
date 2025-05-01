using UnityEngine;
using Status;

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
        playerInventory = FindFirstObjectByType<PlayerInventory>();
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

    public bool TryUnlockSkill(SkillData skill)
    {
        if (CanUnlock(skill) && PlayerInventory.Instance.TrySpendMana(skill.manaCost))
        {
            skill.isUnlocked = true;
            ApplyReward(skill);
            Debug.Log($"Compétence débloquée. Mana restant: {PlayerInventory.Instance.SkillTreeMana}");
            return true;
        }
        return false;
    }

    private void ApplyReward(SkillData skill)
    {
        if (skill.reward == null) return;

        var playerStatus = FindFirstObjectByType<EntityStatus>();
        if (playerStatus == null) return;

        switch (skill.reward.type)
        {
            case Reward.RewardType.HealthIncrease:
                playerStatus.maxHealth += skill.reward.value;
                playerStatus.Heal(skill.reward.value);
                Debug.Log($"Santé maximale augmentée de {skill.reward.value}. Nouvelle santé max: {playerStatus.maxHealth}");
                break;
                
            case Reward.RewardType.ManaIncrease:
                break;
                
            case Reward.RewardType.DamageBoost:
                break;
        }
    }
}
