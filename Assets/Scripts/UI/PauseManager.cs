using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    public GameObject pauseMenuPanel;
    public GameObject optionsPanel;
    public GameObject characterPanel;
    public GameObject hud;

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
        hud.SetActive(false);
        GameManager.Instance.PauseGame();
    }

    public void ResumeGame()
    {
        pauseMenuPanel.SetActive(false);
        optionsPanel.SetActive(false);
        characterPanel.SetActive(false);
        hud.SetActive(true);
        GameManager.Instance.ResumeGame();
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
        pauseMenuPanel.SetActive(false);
        optionsPanel.SetActive(false);
        characterPanel.SetActive(false);
        _ = GameManager.Instance.ReturnToMainMenu();
    }

    public void QuitGame()
    {
        _ = GameManager.Instance.QuitGame();
    }
}
