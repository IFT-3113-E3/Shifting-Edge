using UnityEngine;
using UnityEngine.UI;

public class OptionsManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject optionsMainPanel;
    public GameObject gamePanel;
    public GameObject videoPanel;
    public GameObject audioPanel;

    [Header("Références")]
    public Button backButton;

    private GameObject currentSubPanel;

    private void Start()
    {
        CloseAllSubPanels();
        
        if (backButton != null)
            backButton.onClick.AddListener(BackToMenu);
    }

    public void GameOptions()
    {
        ToggleSubPanel(gamePanel);
    }

    public void VideoOptions()
    {
        ToggleSubPanel(videoPanel);
    }

    public void AudioOptions()
    {
        ToggleSubPanel(audioPanel);
    }

    private void ToggleSubPanel(GameObject panelToActivate)
    {
        // Désactiver le panel actuel s'il y en a un
        if (currentSubPanel != null && currentSubPanel != panelToActivate)
        {
            currentSubPanel.SetActive(false);
        }

        // Activer/désactiver le nouveau panel
        if (panelToActivate != currentSubPanel)
        {
            panelToActivate.SetActive(true);
            currentSubPanel = panelToActivate;
        }
        else
        {
            currentSubPanel = null;
            panelToActivate.SetActive(false);
        }
    }

    private void CloseAllSubPanels()
    {
        if (gamePanel != null) gamePanel.SetActive(false);
        if (videoPanel != null) videoPanel.SetActive(false);
        if (audioPanel != null) audioPanel.SetActive(false);
        currentSubPanel = null;
    }

    public void BackToMenu()
    {
        // Désactiver le panel principal des options
        if (optionsMainPanel != null)
            optionsMainPanel.SetActive(false);
        
        CloseAllSubPanels();
        
        // Activer le panel du menu principal (à assigner dans l'Inspector)
        // FindObjectOfType<MainMenuManager>().mainMenuPanel.SetActive(true);
    }
}