using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class ItemSelection : MonoBehaviour
{
    public Button[] itemButtons;
    public float radius = 200f;
    public Vector2 centerOffset = Vector2.zero;
    public KeyCode toggleKey = KeyCode.Tab;

    void Start()
    {
        ArrangeButtonToCircle();
        SetItemsActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            SetItemsActive(true);
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
        SetItemsActive(false);
    }
}
