using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    // Stockage des items (nom → quantité)
    private Dictionary<string, int> inventory = new Dictionary<string, int>();

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

    // Ajoute un item à l'inventaire virtuel
    public void AddItem(ItemData item, int quantity = 1)
    {
        if (inventory.ContainsKey(item.itemName))
        {
            inventory[item.itemName] += quantity;
        }
        else
        {
            inventory.Add(item.itemName, quantity);
        }
        Debug.Log($"Ajouté à l'inventaire : {quantity}x {item.itemName}");
    }

    // Vérifie si un item est présent
    public bool HasItem(string itemName, int quantity = 1)
    {
        return inventory.ContainsKey(itemName) && inventory[itemName] >= quantity;
    }

    // Affiche le contenu dans la console
    public void PrintInventory()
    {
        foreach (var item in inventory)
        {
            Debug.Log($"{item.Key} x{item.Value}");
        }
    }
}