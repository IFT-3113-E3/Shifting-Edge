using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class LaunchManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject launchPanel;
    public GameObject mainMenuPanel;

    [Header("Transition Settings")]
    public float fadeDuration = 0.5f;

    private bool hasSwitched = false;
    private InputAction anyInputAction;
    private CanvasGroup launchCanvasGroup;
    private CanvasGroup mainMenuCanvasGroup;

    private void Awake()
    {
        // Initialiser les CanvasGroup
        if (launchPanel != null)
        {
            launchCanvasGroup = launchPanel.GetComponent<CanvasGroup>();
            if (launchCanvasGroup == null)
                launchCanvasGroup = launchPanel.AddComponent<CanvasGroup>();

            // Configurer le launchPanel comme transparent au départ
            launchCanvasGroup.alpha = 0f;
            launchCanvasGroup.interactable = false;
            launchCanvasGroup.blocksRaycasts = false;
            launchPanel.SetActive(true); // Gardez-le activé !
        }

        if (mainMenuPanel != null)
        {
            mainMenuCanvasGroup = mainMenuPanel.GetComponent<CanvasGroup>();
            if (mainMenuCanvasGroup == null)
                mainMenuCanvasGroup = mainMenuPanel.AddComponent<CanvasGroup>();

            // Configurer le mainMenuPanel comme invisible au départ
            mainMenuCanvasGroup.alpha = 0f;
            mainMenuCanvasGroup.interactable = false;
            mainMenuCanvasGroup.blocksRaycasts = false;
            mainMenuPanel.SetActive(true); // Gardez-le activé mais transparent !
        }

        // Configurer l'input
        anyInputAction = new InputAction(type: InputActionType.PassThrough);
        anyInputAction.AddBinding("<Gamepad>/<Button>");
        anyInputAction.AddBinding("<Keyboard>/anyKey");
        anyInputAction.AddBinding("<Mouse>/<Button>");
        anyInputAction.performed += _ => OnAnyInput();
    }

    private void Start()
    {
        // Fade-in du launchPanel au démarrage
        if (launchCanvasGroup != null)
        {
            StartCoroutine(FadeInLaunchPanel());
        }
    }

    private IEnumerator FadeInLaunchPanel()
    {
        // Activer les interactions
        launchCanvasGroup.interactable = true;
        launchCanvasGroup.blocksRaycasts = true;

        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            launchCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        launchCanvasGroup.alpha = 1f;
    }

    private void OnAnyInput()
    {
        if (!hasSwitched && launchCanvasGroup.alpha >= 0.99f) // Attendre que le fade-in soit fini
        {
            SwitchToMainMenu();
        }
    }

    private void OnEnable() => anyInputAction.Enable();
    private void OnDisable() => anyInputAction.Disable();

    private void SwitchToMainMenu()
    {
        hasSwitched = true;
        StartCoroutine(TransitionPanels());
    }

    private IEnumerator TransitionPanels()
    {
        // Fade Out du launchPanel
        if (launchCanvasGroup != null)
        {
            float elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                launchCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            launchCanvasGroup.alpha = 0f;
            launchCanvasGroup.interactable = false;
            launchCanvasGroup.blocksRaycasts = false;
        }

        // Fade In du mainMenuPanel
        if (mainMenuCanvasGroup != null)
        {
            mainMenuCanvasGroup.interactable = true;
            mainMenuCanvasGroup.blocksRaycasts = true;

            float elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                mainMenuCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            mainMenuCanvasGroup.alpha = 1f;
        }
    }
}