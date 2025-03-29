using UnityEngine;

public class SkillManager : MonoBehaviour
{
    [Header("Skill Settings")]
    public int maxStacks = 3;
    public int[] skillCosts = new int[3]; // Coût pour chaque compétence

    [Header("References")]
    public SkillHUDController hudController;
    public Animator characterAnimator;

    private int _currentStacks;
    private bool[] _animationTriggered = new bool[3];

    private void Start()
    {
        hudController.Initialize(maxStacks);
    }

    private void Update()
    {
        CheckAnimations();
        HandleSkillInput();
    }

    private void CheckAnimations()
    {
        for (int i = 0; i < 3; i++)
        {
            if (characterAnimator.GetCurrentAnimatorStateInfo(0).IsName($"Attack_{i+1}") && 
                !_animationTriggered[i])
            {
                OnAnimationCompleted(i);
                _animationTriggered[i] = true;
            }
            else if (!characterAnimator.GetCurrentAnimatorStateInfo(0).IsName($"Attack_{i+1}"))
            {
                _animationTriggered[i] = false;
            }
        }
    }

    private void OnAnimationCompleted(int animationIndex)
    {
        if (_currentStacks < maxStacks)
        {
            _currentStacks++;
            hudController.UpdateStacks(_currentStacks);
        }
    }

    private void HandleSkillInput()
    {
        if (Input.GetKeyDown(KeyCode.Q)) TryUseSkill(0);
        if (Input.GetKeyDown(KeyCode.W)) TryUseSkill(1);
        if (Input.GetKeyDown(KeyCode.E)) TryUseSkill(2);
    }

    private void TryUseSkill(int skillIndex)
    {
        if (skillIndex < skillCosts.Length && _currentStacks >= skillCosts[skillIndex])
        {
            _currentStacks -= skillCosts[skillIndex];
            hudController.UpdateStacks(_currentStacks);
            
            // Déclencher l'effet de la compétence ici
            Debug.Log($"Skill {skillIndex+1} used!");
        }
    }
}