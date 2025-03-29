using UnityEngine;
using UnityEngine.UI;

public class OptionsManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject optionsMainPanel;
    public GameObject gamePanel;
    public GameObject videoPanel;
    public GameObject audioPanel;

    [Header("Références")]
    public Button backButton;

    private GameObject currentSubPanel;

    private void Start()
    {
        // Initialisation des panels
        if (optionsMainPanel != null) optionsMainPanel.SetActive(false);
        CloseAllSubPanels();
        
        // Configuration du bouton back
        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners(); // Nettoyage préalable
            backButton.onClick.AddListener(BackToMenu);
        }
        else
        {
            Debug.LogError("Le bouton Back n'est pas assigné dans l'inspecteur !");
        }
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
        // Désactiver le panel actuel s'il existe
        if (currentSubPanel != null) 
        {
            currentSubPanel.SetActive(false);
        }

        // Activer le nouveau panel seulement si différent
        if (panelToActivate != currentSubPanel)
        {
            panelToActivate.SetActive(true);
            currentSubPanel = panelToActivate;
            
            // S'assurer que le panel principal des options est actif
            if (optionsMainPanel != null) 
                optionsMainPanel.SetActive(true);
        }
        else
        {
            currentSubPanel = null;
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
        // 1. Fermer tous les sous-panels
        CloseAllSubPanels();
        
        // 2. Fermer le panel principal des options
        if (optionsMainPanel != null)
            optionsMainPanel.SetActive(false);
        
        // 3. Réactiver le menu principal
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
        else
            Debug.LogError("Le panel du menu principal n'est pas assigné !");
    }
}