using UnityEngine;
using UnityEngine.UI;

public class SkillNode : MonoBehaviour
{
    public bool isUnlocked = false;
    public SkillNode[] requiredNodes;
    public Button button;
    public Color lockedColor = Color.red;
    public Color unlockedColor = Color.green;
    public Color selectableColor = Color.blue;

    void Start()
    {
        button.onClick.AddListener(OnClick);
        UpdateVisual();
    }

    public bool CanUnlock()
    {
        foreach (SkillNode node in requiredNodes)
        {
            if (!node.isUnlocked) return false;
        }
        return true;
    }

    public void OnClick()
    {
        if (!isUnlocked && CanUnlock())
        {
            isUnlocked = true;
            UpdateVisual();
            Debug.Log("Skill débloqué: " + gameObject.name);

            UpdateAllSkills();
        }
    }

    public void UpdateVisual()
    {
        ColorBlock colors = button.colors;

        if (isUnlocked)
        {
            colors.normalColor = unlockedColor;
        }
        else if (CanUnlock())
        {
            colors.normalColor = selectableColor;
        }
        else
        {
            colors.normalColor = lockedColor;
        }

        colors.highlightedColor = colors.normalColor;
        colors.pressedColor = colors.normalColor;
        colors.selectedColor = colors.normalColor;
        colors.disabledColor = colors.normalColor;

        button.colors = colors;
    }

    public void UpdateAllSkills()
    {
        SkillNode[] allSkills = FindObjectsOfType<SkillNode>();

        foreach (SkillNode skill in allSkills)
        {
            skill.UpdateVisual();
        }
    }
}