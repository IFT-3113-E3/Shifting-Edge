using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [SerializeField] private InventorySlot[] slots;
    private InventorySlot selectedSlot;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SelectSlot(InventorySlot slot)
    {
        selectedSlot = slot;
        Debug.Log("Slot sélectionné : " + slot.name);
    }

    public void AddItem(ItemData item)
    {
        if (selectedSlot != null)
        {
            selectedSlot.UpdateSlot(item);
        }
        else
        {
            Debug.Log("Aucun slot sélectionné !");
        }
    }

    public void RemoveSelectedItem()
    {
        if (selectedSlot != null)
        {
            selectedSlot.UpdateSlot(null);
        }
        else
        {
            Debug.Log("Aucun slot sélectionné !");
        }
    }
}