using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    [Header("Item Information")]
    public string itemName;
    public Sprite icon;
    public string description;
    public int stats;

}