using UnityEngine;
using UnityEngine.UI;

public class PauseMenuManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject pauseUI;               // Ton Canvas de menu pause
    public RawImage pauseBlurImage;          // RawImage floue à activer pendant la pause

    [Header("Camera Control")]
    public OrbitCamera orbitCamera;
    // public CameraPauseOffset cameraPauseOffset;  // Ton script d'offset sur la caméra

    private bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;

        pauseUI.SetActive(true);
        pauseBlurImage.enabled = true;

        if (orbitCamera != null)
            orbitCamera.enabled = false;

        // if (cameraPauseOffset != null)
        //     cameraPauseOffset.EnterPause();
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        pauseUI.SetActive(false);
        pauseBlurImage.enabled = false;

        if (orbitCamera != null)
            orbitCamera.enabled = true;

        // if (cameraPauseOffset != null)
        //     cameraPauseOffset.ExitPause();
    }

    public void OnHoverDirection(string direction)
    {
        // if (!isPaused || cameraPauseOffset == null) return;

        // cameraPauseOffset.Hover(direction);
    }
}
