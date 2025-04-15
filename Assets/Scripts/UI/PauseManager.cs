using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    public GameObject pauseMenuPanel;
    public GameObject optionsPanel;
    public GameObject characterPanel;

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
        isPaused = true;
    }

    public void ResumeGame()
    {
        pauseMenuPanel.SetActive(false);
        optionsPanel.SetActive(false);
        characterPanel.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void OpenOptions()
    {
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
    }
}