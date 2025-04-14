using UnityEngine;

public class MapManager : MonoBehaviour
{
    public GameObject minimapCanvas;
    public GameObject fullMapCanvas;
    public KeyCode toggleKey = KeyCode.M;

    private bool isFullMapActive = false;

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            isFullMapActive = !isFullMapActive;
            minimapCanvas.SetActive(!isFullMapActive);
            fullMapCanvas.SetActive(isFullMapActive);
        }
    }
}