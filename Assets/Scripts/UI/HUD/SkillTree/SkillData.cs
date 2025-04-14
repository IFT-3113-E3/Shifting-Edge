using UnityEngine;

[CreateAssetMenu(fileName = "New Skill", menuName = "Skills/Skill Data")]
public class SkillData : ScriptableObject
{
    public string skillName;
    [TextArea] public string description;
    public int manaCost;
    public Sprite icon;
    public SkillData[] prerequisites;

    [System.NonSerialized] public bool isUnlocked = false;
}