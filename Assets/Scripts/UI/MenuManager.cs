using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject optionsPanel;
    public GameObject creditsPanel;

    public void OpenOptions()
    {
        SetActivePanel(optionsPanel);
    }

    public void OpenCredits()
    {
        SetActivePanel(creditsPanel);
    }

    public void QuitGame()
    {
        Debug.Log("Quitter le jeu...");
        Application.Quit();
    }

    private void SetActivePanel(GameObject panelToActivate)
    {
        if (mainMenuPanel != null)
            mainMenuPanel?.SetActive(false);

        if (panelToActivate != null)
            panelToActivate?.SetActive(true);
    }
}
