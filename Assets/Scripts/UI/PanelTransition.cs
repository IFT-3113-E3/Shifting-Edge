using UnityEngine;
using UnityEngine.UI; // Pour les UI Image/Text
using System.Collections;

public class PanelTransition : MonoBehaviour
{
    [Header("Références")]
    public CanvasGroup panelCanvasGroup; // Assignez le CanvasGroup du panel
    public float fadeDuration = 0.5f; // Durée du fade

    private Coroutine currentFadeCoroutine;

    public void TogglePanel(bool show)
    {
        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
        }
        currentFadeCoroutine = StartCoroutine(show ? FadeIn() : FadeOut());
    }

    // Fade In (apparition progressive)
    private IEnumerator FadeIn()
    {
        panelCanvasGroup.blocksRaycasts = true; // Active les interactions
        panelCanvasGroup.interactable = true;

        float elapsedTime = 0f;
        float startAlpha = panelCanvasGroup.alpha;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime; // Ignore le Time.timeScale
            panelCanvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, elapsedTime / fadeDuration);
            yield return null;
        }

        panelCanvasGroup.alpha = 1f;
    }

    // Fade Out (disparition progressive)
    private IEnumerator FadeOut()
    {
        panelCanvasGroup.blocksRaycasts = false; // Désactive les interactions
        panelCanvasGroup.interactable = false;

        float elapsedTime = 0f;
        float startAlpha = panelCanvasGroup.alpha;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            panelCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / fadeDuration);
            yield return null;
        }

        panelCanvasGroup.alpha = 0f;
    }

    public void ResetPanel()
    {
        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
        }
        panelCanvasGroup.alpha = 0f;
        panelCanvasGroup.blocksRaycasts = false;
        panelCanvasGroup.interactable = false;
    }
}