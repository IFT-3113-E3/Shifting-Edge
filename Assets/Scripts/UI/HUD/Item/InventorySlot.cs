using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Image icon;
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(SelectSlot);
    }

    public void UpdateSlot(ItemData item)
    {
        if (item != null)
        {
            icon.sprite = item.icon;
            icon.enabled = true;
        }
        else
        {
            icon.sprite = null;
            icon.enabled = false;
        }
    }

    private void SelectSlot()
    {
        InventoryManager.Instance.SelectSlot(this);
    }
}