using UnityEngine;

public class Minimap : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float height = 20f;
    [SerializeField] private bool rotateWithPlayer = false;

    private void LateUpdate()
    {
        // Positionnement au-dessus du joueur
        transform.position = player.position + Vector3.up * height;
        
        // Rotation (soit fixe, soit suivant le joueur sur l'axe Y seulement)
        if (!rotateWithPlayer)
        {
            transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }
        else
        {
            transform.rotation = Quaternion.Euler(90f, player.eulerAngles.y, 0f);
        }
    }
}