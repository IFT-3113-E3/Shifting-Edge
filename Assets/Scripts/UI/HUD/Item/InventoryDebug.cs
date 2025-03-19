using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryDebug : MonoBehaviour
{
    [Header("Références")]
    public ItemData[] testItems;

    private void Update()
    {
        if (Keyboard.current.zKey.wasPressedThisFrame)
            InventoryManager.Instance.AddItem(testItems[0]);

        if (Keyboard.current.xKey.wasPressedThisFrame)
            InventoryManager.Instance.AddItem(testItems[1]);

        if (Keyboard.current.cKey.wasPressedThisFrame)
            InventoryManager.Instance.AddItem(testItems[2]);

        if (Keyboard.current.dKey.wasPressedThisFrame)
            InventoryManager.Instance.RemoveSelectedItem();
    }
}