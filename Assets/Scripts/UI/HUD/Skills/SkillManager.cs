using UnityEngine;

public class SkillManager : MonoBehaviour
{
    [Header("Skill Settings")]
    public int maxStacks = 3;
    public int[] skillCosts = new int[3]; // Coût pour chaque compétence

    [Header("References")]
    public SkillHUDController hudController;

    private PlayerStats _playerStats;

    private void Start()
    {
        _playerStats = GameManager.Instance.GameSession?.PlayerStats;
        if (_playerStats == null)
        {
            Debug.LogError("PlayerStats is not initialized.");
            return;
        }
        _playerStats.OnAbilityStacksChanged += OnStacksChanged;
        hudController.Initialize(maxStacks);
    }

    private void Update()
    {
        HandleSkillInput();
    }

    private void OnStacksChanged(int numStacks)
    {
        hudController.UpdateStacks(numStacks);
    }

    private void HandleSkillInput()
    {
        if (Input.GetKeyDown(KeyCode.Q)) TryUseSkill(0);
        if (Input.GetKeyDown(KeyCode.W)) TryUseSkill(1);
        if (Input.GetKeyDown(KeyCode.E)) TryUseSkill(2);
    }

    private void TryUseSkill(int skillIndex)
    {
        var currentStacks = _playerStats.abilityStacks;
        if (skillIndex < skillCosts.Length && currentStacks >= skillCosts[skillIndex])
        {
            _playerStats.SetAbilityStacks(currentStacks - skillCosts[skillIndex]);
            
            // Déclencher l'effet de la compétence ici
            Debug.Log($"Skill {skillIndex+1} used!");
        }
    }
}