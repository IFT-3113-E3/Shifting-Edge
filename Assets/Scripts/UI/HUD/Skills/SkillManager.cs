using UnityEngine;

public class SkillManager : MonoBehaviour
{
    [Header("Paramètres Généraux")]
    public int maxStacks = 3;
    public int attacksPerStack = 4;

    [Header("Coûts des Compétences")]
    public int skill1Cost = 1;
    public int skill2Cost = 2;

    [Header("Effets Visuels")]
    public ScreenFlashEffect flashEffect;

    [Header("Références UI")]
    public SkillHUDController hudController;

    private float totalProgress;
    private int attackCounter;

    private void Start()
    {
        totalProgress = 0f;
        attackCounter = 0;
        hudController.Initialize(maxStacks);
    }

    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            attackCounter++;
            UpdateStackProgress();
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            TryUseSkill(skill1Cost, Color.red);
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            TryUseSkill(skill2Cost, Color.blue);
        }
    }

    private void UpdateStackProgress()
    {
        if (totalProgress >= maxStacks) return;

        totalProgress += 1f / attacksPerStack;
        attackCounter = 0;

        hudController.UpdateStacks(totalProgress);
    }

    private void TryUseSkill(int cost, Color color)
    {
        if (totalProgress >= cost)
        {
            totalProgress -= cost;
            hudController.UpdateStacks(totalProgress);
            TriggerSkillEffect(color);
        }
    }

    private void TriggerSkillEffect(Color color)
    {
        // Déclencher l'effet de flash
        if (flashEffect != null) flashEffect.TriggerFlash(color);
    }
}