using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChestItemIndicator : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI itemName;

    public void Initialize(ItemData item, float duration)
    {
        icon.sprite = item.icon;
        itemName.text = $"+ {item.itemName}";

        Destroy(gameObject, duration);
    }
}
