using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
<<<<<<< Updated upstream
    [Header("UI Elements")]
    public GameObject pauseUI;               // Ton Canvas de menu pause
    public RawImage pauseBlurImage;          // RawImage floue à activer pendant la pause

    [Header("Camera Control")]
    public OrbitCamera orbitCamera;
    // public CameraPauseOffset cameraPauseOffset;  // Ton script d'offset sur la caméra
=======
    public GameObject pauseMenuPanel;
    public GameObject optionsPanel;
    public GameObject characterPanel;
>>>>>>> Stashed changes

    private bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pauseMenuPanel.activeSelf)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        pauseMenuPanel.SetActive(true);
        Time.timeScale = 0f;
<<<<<<< Updated upstream

        pauseUI.SetActive(true);
        pauseBlurImage.enabled = true;

        if (orbitCamera != null)
            orbitCamera.enabled = false;

        // if (cameraPauseOffset != null)
        //     cameraPauseOffset.EnterPause();
=======
        isPaused = true;
>>>>>>> Stashed changes
    }

    public void ResumeGame()
    {
        pauseMenuPanel.SetActive(false);
        optionsPanel.SetActive(false);
        characterPanel.SetActive(false);
        Time.timeScale = 1f;
<<<<<<< Updated upstream

        pauseUI.SetActive(false);
        pauseBlurImage.enabled = false;

        if (orbitCamera != null)
            orbitCamera.enabled = true;

        // if (cameraPauseOffset != null)
        //     cameraPauseOffset.ExitPause();
=======
        isPaused = false;
>>>>>>> Stashed changes
    }

    public void OpenOptions()
    {
<<<<<<< Updated upstream
        // if (!isPaused || cameraPauseOffset == null) return;

        // cameraPauseOffset.Hover(direction);
=======
        optionsPanel.SetActive(true);
        pauseMenuPanel.SetActive(false);
    }

    public void OpenCharacterPanel()
    {
        characterPanel.SetActive(true);
        pauseMenuPanel.SetActive(false);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("StartMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
>>>>>>> Stashed changes
    }
}
