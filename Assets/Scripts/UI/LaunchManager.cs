using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class LaunchScreen : MonoBehaviour
{
    [Header("Panels")]
    public GameObject launchPanel;
    public GameObject mainMenuPanel;

    private bool hasSwitched = false;
    private InputAction anyInputAction;

    private void Awake()
    {
        // Configuration de l'action qui écoute tous les inputs
        anyInputAction = new InputAction(type: InputActionType.PassThrough);
        anyInputAction.AddBinding("<Gamepad>/<Button>");  // Tous les boutons gamepad
        anyInputAction.AddBinding("<Keyboard>/anyKey");   // Toutes les touches clavier
        anyInputAction.AddBinding("<Mouse>/<Button>");    // Tous les boutons souris

        anyInputAction.performed += _ => OnAnyInput();
    }

    private void OnAnyInput()
    {
        if (!hasSwitched)
        {
            SwitchToMainMenu();
        }
    }

    private void OnEnable() => anyInputAction.Enable();
    private void OnDisable() => anyInputAction.Disable();

    private void SwitchToMainMenu()
    {
        hasSwitched = true;
        launchPanel?.SetActive(false);
        mainMenuPanel?.SetActive(true);
        Debug.Log("Passage au menu principal !");
    }
}