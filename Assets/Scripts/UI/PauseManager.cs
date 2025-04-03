using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class PauseManager : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private GameObject pauseCanvas; // Assignez votre Canvas pause
    [SerializeField] private GameObject settingsPanelPrefab; // Prefab des paramètres

    private GameObject currentSettingsPanel;
    private bool isPaused = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Touche Echap pressée !");
            TogglePause();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        
        // Gestion du temps et du canvas
        Time.timeScale = isPaused ? 0f : 1f;
        pauseCanvas.SetActive(isPaused);
        
        // Optionnel : Verrouillage curseur
        //Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
        //Cursor.visible = isPaused;
    }

    // Bouton "Reprendre"
    public void ResumeGame()
    {
        TogglePause(); // Désactive le canvas et enlève la pause
    }

    // Bouton "Paramètres"
    public void OpenSettings()
    {
        if (currentSettingsPanel == null)
        {
            currentSettingsPanel = Instantiate(settingsPanelPrefab, pauseCanvas.transform);
        }
        else
        {
            currentSettingsPanel.SetActive(true);
        }
    }

    // Bouton "Menu Principal"
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f; // Réactive le temps avant de changer de scène
        SceneManager.LoadScene("StartMenu");
    }

    // Bouton "Quitter"
    public void QuitGame()
    {
        Application.Quit();
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Pour le mode éditeur
        #endif
    }
}