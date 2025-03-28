using UnityEngine;
using System.Collections;

public class PanelTransition : MonoBehaviour
{
    [Header("Settings")]
    public float fadeInDuration = 1f;
    public float fadeOutDuration = 0.5f;
    public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private CanvasGroup _cg;
    private Coroutine _currentTransition;

    private void Awake()
    {
        _cg = GetComponent<CanvasGroup>();
        _cg.alpha = 0; // Start hidden
    }

    public void TogglePanel(bool show)
    {
        if (_currentTransition != null) StopCoroutine(_currentTransition);
        _currentTransition = StartCoroutine(show ? FadeIn() : FadeOut());
    }

    private IEnumerator FadeIn()
    {
        _cg.interactable = true;
        _cg.blocksRaycasts = true;

        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            _cg.alpha = fadeCurve.Evaluate(elapsed / fadeInDuration);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        _cg.alpha = 1;
    }

    private IEnumerator FadeOut()
    {
        _cg.interactable = false;
        _cg.blocksRaycasts = false;

        float elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            _cg.alpha = 1 - fadeCurve.Evaluate(elapsed / fadeOutDuration);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        _cg.alpha = 0;
    }

    public bool IsDoneFading() => _cg.alpha == 0f || _cg.alpha == 1f;
}