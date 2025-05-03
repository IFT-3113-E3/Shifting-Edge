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

    private void Update()
    {
        HandleKeyboardUnlock();
    }

    private void HandleKeyboardUnlock()
    {
        if (allSkills == null || allSkills.Length == 0) return;

        // Débloquer avec les touches 1-9
        for (int i = 0; i < Mathf.Min(allSkills.Length, 9); i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                TryUnlockSkill(allSkills[i]);
                Debug.Log($"Compétence {allSkills[i].skillName} débloquée");
            }
        }

        // Option: Touche R pour réinitialiser
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetAllSkills();
            Debug.Log("Toutes les compétences ont été réinitialisées");
        }
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
        var playerController = FindFirstObjectByType<PlayerController>();
        if (playerStatus == null) return;

        switch (skill.reward.type)
        {
            case Reward.RewardType.HealthIncrease:
                playerStatus.maxHealth += skill.reward.value;
                playerStatus.Heal(skill.reward.value);
                Debug.Log($"Santé maximale augmentée de {skill.reward.value}. Nouvelle santé max: {playerStatus.maxHealth}");
                break;

            case Reward.RewardType.MaxRollsIncrease:
                playerController.MaxConsecutiveRolls += (int)skill.reward.value;
                Debug.Log($"Nombre max de rolls augmenté de {skill.reward.value}. Nouveau max: {playerController.MaxConsecutiveRolls}");
                break;

            case Reward.RewardType.RollCooldownReduction:
                playerController.RollCooldownTime -= skill.reward.value;
                Debug.Log($"Cooldown de roll réduit de {skill.reward.value}. Nouveau cooldown: {playerController.RollCooldownTime}");
                break;

            case Reward.RewardType.XPBoost:
                XPManager.Instance.AddXPMultiplier(skill.reward.value);
                Debug.Log($"Bonus d'XP de {skill.reward.value*100}% appliqué");
                break;
                
            case Reward.RewardType.DamageBoost:
                break;
        }
    }

    private void ReapplyAllRewards()
    {
        var playerController = FindFirstObjectByType<PlayerController>();
        if (playerController == null) return;

        // Réinitialiser toutes les valeurs avant de réappliquer
        playerController.ResetCooldownToBase();

        foreach (var skill in allSkills)
        {
            if (skill.isUnlocked)
            {
                ApplyReward(skill);
            }
        }
    }
}
