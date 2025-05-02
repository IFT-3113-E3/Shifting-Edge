using UnityEngine;
using UnityEngine.Rendering.Universal.Internal;
using UnityEngine.UI;

[ExecuteInEditMode]
public class ItemSelection : MonoBehaviour
{
    public Button[] itemButtons;
    public float radius = 200f;
    public Vector2 centerOffset = Vector2.zero;
    public KeyCode toggleKey = KeyCode.Tab; // Touche pour afficher/masquer les items

    void Start()
    {
        ArrangeButtonToCircle();
        SetItemsActive(false); // Masquer les items au démarrage
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            SetItemsActive(true); // Afficher les items
        }
    }

    void OnValidate()
    {
        ArrangeButtonToCircle();
    }

    void ArrangeButtonToCircle()
    {
        if (itemButtons == null || itemButtons.Length == 0)
            return;

        float angleStep = 360f / itemButtons.Length;

        for (int i = 0; i < itemButtons.Length; i++)
        {
            if (itemButtons[i] == null)
                continue;

            float angle = angleStep * i;
            Vector2 buttonPosition = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * radius + centerOffset;
            if (!itemButtons[i].TryGetComponent<RectTransform>(out var rectTransform))
                continue;
            rectTransform.anchoredPosition = buttonPosition;
            // rectTransform.localEulerAngles = new Vector3(0, 0, -angle);

            // foreach (var image in itemButtons[i].GetComponentsInChildren<Image>())
            // {
            //     image.rectTransform.sizeDelta = new Vector2(radius, radius);
            // }
        }
    }

    void SetItemsActive(bool isActive)
    {
        foreach (var button in itemButtons)
        {
            if (button != null)
                button.gameObject.SetActive(isActive);
        }
    }

    public void OnItemClicked()
    {
        SetItemsActive(false); // Masquer les items lorsqu'un item est cliqué
    }
}
