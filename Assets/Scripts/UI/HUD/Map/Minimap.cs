using UnityEngine;

public class Minimap : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float height = 20f;

    private void LateUpdate()
    {
        // Positionnement au-dessus du joueur et rotation de la caméra pour qu'elle regarde vers le bas
        transform.SetPositionAndRotation(player.position + Vector3.up * height, Quaternion.Euler(90f, 270f, 0f));
    }
}
