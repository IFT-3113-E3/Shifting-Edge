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
    
    [Header("Collectible assets")]
    public CollectibleData[] collectibles;
    
    // [Header("UI assets")]
    // public GameObject startMenuPrefab;
    
    private Dictionary<string, WorldSection> _worldSectionDictionary;
    private Dictionary<string, CollectibleData> _collectibleDictionary;

    private void OnEnable()
    {
        if (worldSections != null && worldSections.Length != 0)
        {
            _worldSectionDictionary = new Dictionary<string, WorldSection>();
            foreach (var section in worldSections)
            {
                if (!_worldSectionDictionary.TryAdd(section.sectionId, section))
                {
                    Debug.LogError($"Duplicate world section ID found: {section.sectionId}");
                }
            }
        }
        
        if (collectibles != null && collectibles.Length != 0)
        {
            _collectibleDictionary = new Dictionary<string, CollectibleData>();
            foreach (var collectible in collectibles)
            {
                if (!_collectibleDictionary.TryAdd(collectible.id, collectible))
                {
                    Debug.LogError($"Duplicate collectible ID found: {collectible.id}");
                }
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
    
    public CollectibleData GetCollectible(string collectibleId)
    {
        if (_collectibleDictionary.TryGetValue(collectibleId, out var collectible))
        {
            return collectible;
        }

        Debug.LogError($"Collectible with ID {collectibleId} not found.");
        return null;
    }
}