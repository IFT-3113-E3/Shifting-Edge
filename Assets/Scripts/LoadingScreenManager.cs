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

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        HideImmediate();
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

        HideImmediate();
    }

    public void SetProgress(float progress)
    {
        progressBar.value = Mathf.Clamp01(progress);
    }

    private void HideImmediate()
    {
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
}