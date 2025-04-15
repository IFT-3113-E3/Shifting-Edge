using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    private Transform player;

    void Start()
    {
        if (player == null)
        {
            GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");
            if (foundPlayer != null)
                player = foundPlayer.transform;        }
    }

    void LateUpdate()
    {
        if (player != null)
        {
            transform.position = new Vector3(player.position.x, player.position.y + 10, player.position.z);
        }
    }
}
