using UnityEngine;

public class Minimap : MonoBehaviour
{
    public Transform player;
    public float height = 20f;

    private void LateUpdate()
    {
        Vector3 newPosition = player.position;
        newPosition.y = height;
        transform.position = newPosition;

        transform.rotation = Quaternion.Euler(90f, player.eulerAngles.y, 0f);
    }
}