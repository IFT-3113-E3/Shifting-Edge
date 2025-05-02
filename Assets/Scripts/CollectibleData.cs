using UnityEngine;

[CreateAssetMenu(menuName = "Game/Collectibles/Collectible")]
public class CollectibleData : ScriptableObject
{
    [Tooltip("Unique ID used to track collection status across saves.")]
    public string id;

    [Tooltip("Display name for UI.")]
    public string displayName;

    [TextArea]
    public string description;

    [Tooltip("Icon for menus or inventory UI.")]
    public Sprite icon;
}