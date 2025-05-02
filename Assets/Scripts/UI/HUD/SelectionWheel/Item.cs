using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Item : MonoBehaviour//, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public string itemName;
    public Sprite itemIcon;

    // private string itemNameText = "Name";
    // private string itemPreview = "Selected";

    // private Text itemText;
    // private Image itemImage;

    private static Item selectedItem = null;

    // void Start()
    // {
    //     if (!string.IsNullOrEmpty(itemName))
    //     {
    //         var itemNameTransform = transform.Find(itemNameText);
    //         if (itemNameTransform != null && itemNameTransform.TryGetComponent(out itemText))
    //         {
    //             itemText.text = itemName;
    //         }
    //     }

    //     if (!string.IsNullOrEmpty(itemPreview))
    //     {
    //         var itemPreviewTransform = transform.Find(itemPreview);
    //         if (itemPreviewTransform != null && itemPreviewTransform.TryGetComponent(out itemImage))
    //         {
    //             itemImage.sprite = itemIcon;
    //         }
    //     }
    // }

    // public void OnPointerEnter(PointerEventData eventData)
    // {
    //     if (selectedItem != null && selectedItem != this)
    //     {
    //         selectedItem.DeselectItem();
    //     }

    //     selectedItem = this;
    // }

    // public void OnPointerExit(PointerEventData eventData)
    // {
    //     // Handle pointer exit event
    //     Debug.Log("Pointer exited item: " + gameObject.name);
    // }
    // public void OnPointerClick(PointerEventData eventData)
    // {
    //     // Handle pointer click event
    //     Debug.Log("Item clicked: " + gameObject.name);
    // }

    // private void SelectItem()
    // {
    //     // Logic to select the item
    //     Debug.Log("Item selected: " + gameObject.name);
    // }

    // private void DeselectItem()
    // {
    //     // Logic to deselect the item
    //     Debug.Log("Item deselected: " + gameObject.name);
    // }
}
