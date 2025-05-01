using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadingScreenManager : MonoBehaviour
{
    public static LoadingScreenManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text loadingText;
    [SerializeField] private Slider progressBar;
    
    [SerializeField] private CanvasGroup toastGroup;
    [SerializeField] private TMP_Text   toastText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        HideLoadingScreenImmediate();
        HideToastImmediate();
    }

    public async Task Show(string message = "Loading...", float fadeDuration = 0.3f)
    {
        loadingText.text = message;
        progressBar.value = 0f;

        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Clamp01(t / fadeDuration);
            await Task.Yield();
        }

        canvasGroup.alpha = 1;
    }

    public async Task Hide(float fadeDuration = 0.3f)
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = 1f - Mathf.Clamp01(t / fadeDuration);
            await Task.Yield();
        }

        HideLoadingScreenImmediate();
    }

    public void SetProgress(float progress)
    {
        progressBar.value = Mathf.Clamp01(progress);
    }

    private void HideToastImmediate()
    {
        toastGroup.alpha          = 0;
        toastGroup.interactable   = false;
        toastGroup.blocksRaycasts = false;
    }
    
    private void HideLoadingScreenImmediate()
    {
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
    
    public async Task ShowToastAsync(string message,
        float displayTime  = 2f,
        float fadeDuration = 0.3f)
    {
        toastText.text = message;
        toastGroup.alpha          = 0;
        toastGroup.interactable   = false;
        toastGroup.blocksRaycasts = false;

        // fade in
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            toastGroup.alpha = Mathf.Lerp(0, 1, t / fadeDuration);
            await Task.Yield();
        }

        // hold
        await Task.Delay(TimeSpan.FromSeconds(displayTime));

        // fade out
        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            toastGroup.alpha = Mathf.Lerp(1, 0, t / fadeDuration);
            await Task.Yield();
        }
        
        // hide
        HideToastImmediate();
    }
}