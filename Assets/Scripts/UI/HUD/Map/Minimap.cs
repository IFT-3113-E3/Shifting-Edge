using UnityEngine;

namespace UI.HUD.Map
{
    public class Minimap : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private float height = 20f;

        private void Start()
        {
            GameManager.Instance.PlayerManager.OnPlayerSpawned += OnPlayerSpawned;
            if (GameManager.Instance.PlayerManager.IsPlayerSpawned)
            {
                OnPlayerSpawned(GameManager.Instance.PlayerManager.Player);
            }
        }
    
        private void OnDestroy()
        {
            GameManager.Instance.PlayerManager.OnPlayerSpawned -= OnPlayerSpawned;
        }

        private void OnPlayerSpawned(Player playerInstance)
        {
            player = playerInstance.transform;
        }
    
        private void LateUpdate()
        {
            if (!player)
            {
                return;
            }

            var physicalCam = CameraManager.Instance.GetCamera().transform;
            var xzProjectedCamLookDirection = Vector3.ProjectOnPlane(physicalCam.forward, Vector3.up);
            // Positionnement au-dessus du joueur et rotation de la caméra pour qu'elle regarde vers le bas
            transform.SetPositionAndRotation(player.position + Vector3.up * height, Quaternion.LookRotation(Vector3.down, xzProjectedCamLookDirection));
        }
    }
}
