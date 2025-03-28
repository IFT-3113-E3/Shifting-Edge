using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class LaunchManager : MonoBehaviour
{
    // Références vers les scripts de transition de chaque panel
    public PanelTransition launchScreen;
    public PanelTransition mainMenu;

    private InputAction _anyKeyInput;

    private void Awake()
    {
        // Configuration de l'input
        _anyKeyInput = new InputAction(binding: "<Keyboard>/anyKey");
        _anyKeyInput.performed += _ => SwitchToMainMenu();
    }

    private void Start()
    {
        // Au démarrage : fade-in du launch screen
        launchScreen.TogglePanel(true);
    }

    public void SwitchToMainMenu()
    {
        StartCoroutine(TransitionToPanel(launchScreen, mainMenu));
    }

    // Méthode générique pour passer d'un panel à un autre
    private IEnumerator TransitionToPanel(PanelTransition fromPanel, PanelTransition toPanel)
    {
        // Fade-out de l'écran actuel
        fromPanel.TogglePanel(false);
        
        // Attendre que le fade-out soit terminé (optionnel)
        yield return new WaitUntil(() => fromPanel.IsDoneFading());
        
        // Fade-in du nouvel écran
        toPanel.TogglePanel(true);
    }

    private void OnEnable() => _anyKeyInput.Enable();
    private void OnDisable() => _anyKeyInput.Disable();
}