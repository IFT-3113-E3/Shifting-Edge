using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    [Header("Item Information")]
    public string itemName;
    public Sprite icon;
    public string description;
    public int stats;

    [Header("Stacking")]
    public bool stackable = true;
    public int maxStack = 1;

    [Header("Item Type")]
    public ItemType itemType;
    public enum ItemType
    {
        Consumable,
        Weapon,
        Armor,
        QuestItem,
    }
}