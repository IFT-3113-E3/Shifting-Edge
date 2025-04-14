using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SceneTransition : MonoBehaviour
{
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1f;

    private void Start()
    {
        FadeIn();
    }

    public void FadeIn()
    {
        StartCoroutine(Fade(1, 0));
    }

    public void FadeOut()
    {
        StartCoroutine(Fade(0, 1));
    }

    private IEnumerator Fade(float from, float to)
    {
        fadeImage.gameObject.SetActive(true);
        float elapsed = 0;

        while (elapsed < fadeDuration)
        {
            fadeImage.color = new Color(0, 0, 0, Mathf.Lerp(from, to, elapsed / fadeDuration));
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        fadeImage.color = new Color(0, 0, 0, to);
        fadeImage.gameObject.SetActive(to > 0);
    }
}