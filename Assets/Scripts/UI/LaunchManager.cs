using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class LaunchManager : MonoBehaviour
{
    // Références aux panels
    public GameObject launchScreen;
    public GameObject mainMenu;

    private InputAction _anyInput;

    private void Awake()
    {
        // Configuration de l'input pour clavier ET souris
        _anyInput = new InputAction(binding: "/*/<button>"); // Tous les boutons (clavier + souris)
        _anyInput.performed += _ => SwitchToMainMenu();
    }

    private void Start()
    {
        // Vérification des références
        if (launchScreen == null || mainMenu == null)
        {
            Debug.LogError("Les panels ne sont pas assignés dans l'inspecteur !");
            return;
        }

        // Initialisation de l'UI
        launchScreen.SetActive(true);
        mainMenu.SetActive(false);
    }

    public void SwitchToMainMenu()
    {
        if (!launchScreen.activeSelf) return; // Évite les déclenchements multiples

        launchScreen.SetActive(false);
        mainMenu.SetActive(true);
        
        // Optionnel : Focus sur le premier élément du menu
        EventSystem.current.SetSelectedGameObject(mainMenu.transform.GetChild(0).gameObject);
    }

    private void OnEnable() => _anyInput.Enable();
    private void OnDisable() => _anyInput.Disable();
}