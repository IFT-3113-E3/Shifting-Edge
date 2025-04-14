using UnityEngine;
using UnityEngine.UI;
using TMPro; // Pour TextMeshPro (optionnel)

public class InventorySlot : MonoBehaviour
{
    public Image icon;
    public Button button;
    public TextMeshProUGUI quantityText; // Texte pour afficher la quantité

    public ItemData currentItem;
    public int currentQuantity = 1;

    public void UpdateSlot(ItemData item, int quantity = 1)
    {
        currentItem = item;
        currentQuantity = quantity;

        if (item != null)
        {
            icon.sprite = item.icon;
            icon.enabled = true;

            // Afficher la quantité si l'item est empilable
            if (item.stackable && quantity > 1)
            {
                quantityText.text = quantity.ToString();
                quantityText.enabled = true;
            }
            else
            {
                quantityText.enabled = false;
            }
        }
        else
        {
            icon.sprite = null;
            icon.enabled = false;
            quantityText.enabled = false;
        }
    }
}