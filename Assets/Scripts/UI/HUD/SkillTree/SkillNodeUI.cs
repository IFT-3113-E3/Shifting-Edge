using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillNodeUI : MonoBehaviour
{
    // Références
    public SkillData skillData;
    private Button button;
    private Image buttonImage;
    [SerializeField] private TextMeshProUGUI skillNameText;
    
    [Header("Couleurs")]
    public Color lockedColor = Color.gray;
    public Color unlockedColor = Color.green;
    public Color canUnlockColor = Color.yellow;

    void Start()
    {
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();
        skillNameText = GetComponentInChildren<TextMeshProUGUI>(); // Récupère automatiquement le texte
        
        button.transition = Selectable.Transition.None;
        
        // Affiche le nom de la compétence
        skillNameText.text = skillData.skillName;
        
        UpdateVisual();
        button.onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        if (!skillData.isUnlocked && SkillTreeManager.Instance.CanUnlock(skillData))
        {
            if (SkillTreeManager.Instance.TryUnlockSkill(skillData))
            {
                UpdateVisual();
                UpdateDependentSkills();
            }
        }
    }

    void UpdateVisual()
    {
        if (skillData.isUnlocked)
        {
            buttonImage.color = unlockedColor;
            skillNameText.color = Color.black; // Texte noir sur fond vert
        }
        else if (SkillTreeManager.Instance.CanUnlock(skillData))
        {
            buttonImage.color = canUnlockColor;
            skillNameText.color = Color.black; // Texte noir sur fond jaune
        }
        else
        {
            buttonImage.color = lockedColor;
            skillNameText.color = Color.white; // Texte blanc sur fond gris
        }
    }

    void UpdateDependentSkills()
    {
        foreach (var skillUI in FindObjectsOfType<SkillNodeUI>())
        {
            if (!skillUI.skillData.isUnlocked)
            {
                skillUI.UpdateVisual();
            }
        }
    }
}