using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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
    public Button gameOptionsButton;
    public Button videoOptionsButton;
    public Button audioOptionsButton;

    private void Start()
    {
        // Initialisation
        TogglePanel(mainMenuPanel, true);
        TogglePanel(optionsMainPanel, false);
        TogglePanel(gamePanel, false);
        TogglePanel(videoPanel, false);
        TogglePanel(audioPanel, false);

        // Configuration des boutons
        backButton.onClick.AddListener(() => {
            TogglePanel(optionsMainPanel, false);
            TogglePanel(mainMenuPanel, true);
        });

        gameOptionsButton.onClick.AddListener(() => TogglePanel(gamePanel));
        videoOptionsButton.onClick.AddListener(() => TogglePanel(videoPanel));
        audioOptionsButton.onClick.AddListener(() => TogglePanel(audioPanel));
    }

    // Méthode Toggle de base
    public void TogglePanel(GameObject panel)
    {
        if (panel != null)
        {
            bool newState = !panel.activeSelf;
            panel.SetActive(newState);
            
            // Gestion spéciale pour les sous-panels options
            if (panel == gamePanel || panel == videoPanel || panel == audioPanel)
            {
                if (newState) 
                {
                    // Désactive les autres sous-panels quand on en active un
                    if (panel != gamePanel) TogglePanel(gamePanel, false);
                    if (panel != videoPanel) TogglePanel(videoPanel, false);
                    if (panel != audioPanel) TogglePanel(audioPanel, false);
                    
                    // Active le panel parent si nécessaire
                    TogglePanel(optionsMainPanel, true);
                }
            }
        }
    }

    // Méthode Toggle avec état forcé
    public void TogglePanel(GameObject panel, bool state)
    {
        if (panel != null)
        {
            panel.SetActive(state);
        }
    }
}