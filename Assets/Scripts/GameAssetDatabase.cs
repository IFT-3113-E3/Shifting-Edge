using System.Collections.Generic;
using Enemy.IceBoss;
using UnityEngine;
using World;

[CreateAssetMenu(fileName = "GameAssetDatabase", menuName = "Game/GameAssetDatabase")]
public class GameAssetDatabase : ScriptableObject
{
    [Header("Player assets")]
    public Player playerPrefab;
    
    [Header("World assets")]
    public WorldSection[] worldSections;
    
    // [Header("UI assets")]
    // public GameObject startMenuPrefab;
    
    private Dictionary<string, WorldSection> _worldSectionDictionary;

    private void OnEnable()
    {
        if (worldSections == null || worldSections.Length == 0)
        {
            return;
        }
        
        _worldSectionDictionary = new Dictionary<string, WorldSection>();
        foreach (var section in worldSections)
        {
            if (!_worldSectionDictionary.TryAdd(section.sectionId, section))
            {
                Debug.LogError($"Duplicate world section ID found: {section.sectionId}");
            }
        }
    }
    
    public WorldSection GetWorldSection(string sectionId)
    {
        if (_worldSectionDictionary.TryGetValue(sectionId, out var section))
        {
            return section;
        }

        Debug.LogError($"World section with ID {sectionId} not found.");
        return null;
    }
}