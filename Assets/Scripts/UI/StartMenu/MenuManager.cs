using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class MenuManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private CanvasGroup launchPanel;
    [SerializeField] private CanvasGroup mainMenuPanel;
    [SerializeField] private CanvasGroup optionsPanel;
    [SerializeField] private CanvasGroup creditsPanel;
    [SerializeField] private CanvasGroup[] optionsSubPanels;

    [Header("Settings")]
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private bool useAnyInputForLaunch = true;
    [SerializeField] private string gameSceneName = "MainScene";
    [SerializeField] private float sceneLoadDelay = 0.5f;

    [Header("Events")]
    public UnityEvent OnTransitionStart;
    public UnityEvent OnTransitionComplete;

    private bool isTransitioning;

    private void Start()
    {
        InitializeUI();
        
        if (useAnyInputForLaunch)
        {
            SetPanel(launchPanel, true);
            SetPanel(mainMenuPanel, false);
        }
    }

    private void Update()
    {
        if (useAnyInputForLaunch && launchPanel.gameObject.activeSelf && !isTransitioning)
        {
            if (Keyboard.current.anyKey.wasPressedThisFrame || 
                Mouse.current.leftButton.wasPressedThisFrame ||
                (Gamepad.current != null && Gamepad.current.aButton.wasPressedThisFrame))
            {
                StartCoroutine(Transition(launchPanel, mainMenuPanel));
            }
        }
    }

    private void InitializeUI()
    {
        if (!useAnyInputForLaunch)
        {
            SetPanel(launchPanel, true);
            SetPanel(mainMenuPanel, false);
        }
        SetPanel(optionsPanel, false);
        SetPanel(creditsPanel, false);
        foreach (var panel in optionsSubPanels) SetPanel(panel, false);
    }

    // ===== BOUTONS PRINCIPAUX =====
    public void OnPlayClicked()
    {
        if (isTransitioning) return;
        StartCoroutine(LoadGameAfterTransition(mainMenuPanel));
    }

    public void OnOptionsClicked()
    {
        if (isTransitioning) return;
        StartCoroutine(Transition(mainMenuPanel, optionsPanel));
    }

    public void OnCreditsClicked()
    {
        if (isTransitioning) return;
        StartCoroutine(Transition(mainMenuPanel, creditsPanel));
    }

    public void OnQuitClicked()
    {
        Application.Quit();
    }

    // ===== BOUTONS RETOUR =====

    public void BackFromCredits()
    {
        if (isTransitioning) return;
        StartCoroutine(Transition(creditsPanel, mainMenuPanel));
    }

public void BackFromOptions()
{
    if (isTransitioning) return;
    StartCoroutine(BackFromOptionsRoutine());
}

private IEnumerator BackFromOptionsRoutine()
{
    isTransitioning = true;
    OnTransitionStart?.Invoke();

    // Fade out du sous-panel actif (le premier trouvé)
    foreach (var panel in optionsSubPanels)
    {
        if (panel.gameObject.activeSelf)
        {
            yield return StartCoroutine(FadeOut(panel));
            break;
        }
    }

    // Fade out du panel principal et in du menu
    yield return StartCoroutine(FadeOut(optionsPanel));
    yield return StartCoroutine(FadeIn(mainMenuPanel));

    isTransitioning = false;
    OnTransitionComplete?.Invoke();
}

    // ===== BOUTONS SOUS-OPTIONS =====
    public void ShowGameOptions()
    {
        if (isTransitioning) return;
        StartCoroutine(ShowSubPanel(optionsSubPanels[0]));
    }

    public void ShowAudioOptions()
    {
        if (isTransitioning) return;
        StartCoroutine(ShowSubPanel(optionsSubPanels[1]));
    }

    public void ShowVideoOptions()
    {
        if (isTransitioning) return;
        StartCoroutine(ShowSubPanel(optionsSubPanels[2]));
    }

    // ===== COROUTINES PRINCIPALES =====
    private IEnumerator LoadGameAfterTransition(CanvasGroup panelOut)
    {
        yield return StartCoroutine(FadeOut(panelOut));
        yield return new WaitForSecondsRealtime(sceneLoadDelay);
        SceneManager.LoadScene(gameSceneName);
    }

    private IEnumerator ShowSubPanel(CanvasGroup subPanel)
    {
        // Fade out tous les autres sous-panels
        foreach (var panel in optionsSubPanels)
        {
            if (panel != subPanel && panel.gameObject.activeSelf)
            {
                yield return StartCoroutine(FadeOut(panel));
            }
        }
        
        // Fade in du sous-panel sélectionné
        yield return StartCoroutine(FadeIn(subPanel));
    }

    private IEnumerator Transition(CanvasGroup outPanel, CanvasGroup inPanel)
    {
        yield return StartCoroutine(FadeOut(outPanel));
        yield return StartCoroutine(FadeIn(inPanel));
    }

    // ===== FONCTIONS DE FADE =====
    private IEnumerator FadeOut(CanvasGroup panel)
    {
        isTransitioning = true;
        OnTransitionStart?.Invoke();

        panel.interactable = false;
        float startAlpha = panel.alpha;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            panel.alpha = Mathf.Lerp(startAlpha, 0, elapsed / fadeDuration);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        panel.alpha = 0;
        panel.blocksRaycasts = false;
        panel.gameObject.SetActive(false);

        isTransitioning = false;
        OnTransitionComplete?.Invoke();
    }

    private IEnumerator FadeIn(CanvasGroup panel)
    {
        isTransitioning = true;
        OnTransitionStart?.Invoke();

        panel.gameObject.SetActive(true);
        panel.blocksRaycasts = true;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            panel.alpha = Mathf.Lerp(0, 1, elapsed / fadeDuration);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        panel.alpha = 1;
        panel.interactable = true;

        isTransitioning = false;
        OnTransitionComplete?.Invoke();
    }

    private void SetPanel(CanvasGroup panel, bool active)
    {
        if (panel == null) return;
        
        panel.alpha = active ? 1 : 0;
        panel.interactable = active;
        panel.blocksRaycasts = active;
        panel.gameObject.SetActive(active);
    }
}