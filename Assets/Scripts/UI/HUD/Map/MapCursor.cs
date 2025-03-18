using UnityEngine;
using UnityEngine.UI;

public class MapCursor : MonoBehaviour
{
    public RectTransform mapRect;
    public RectTransform cursor;

    private void Update()
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            mapRect, Input.mousePosition, null, out Vector2 localPoint))
        {
            cursor.anchoredPosition = localPoint;
        }
    }
}