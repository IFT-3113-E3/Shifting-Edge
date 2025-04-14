using TMPro;
using UnityEngine;

public class ManaDisplay : MonoBehaviour
{
    public TextMeshProUGUI manaText;
    
    void Update()
    {
        if (PlayerInventory.Instance != null)
        {
            manaText.text = $"Mana: {PlayerInventory.Instance.SkillTreeMana}";
        }
    }
}