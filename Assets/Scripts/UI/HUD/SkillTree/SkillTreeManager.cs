using UnityEngine;

public class SkillTreeManager : MonoBehaviour
{
    public SkillNode[] allNodes;

    void Start()
    {
        foreach (SkillNode node in allNodes)
        {
            node.UpdateVisual();
        }
    }

    public void ResetTree()
    {
        foreach (SkillNode node in allNodes)
        {
            node.isUnlocked = false;
            node.UpdateVisual();
        }
    }
}