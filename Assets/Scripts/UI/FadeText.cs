using UnityEngine;
using TMPro;
using System.Collections;

public class FadeTextManual : MonoBehaviour
{
    public TMP_Text text;
    public float fadeDuration = 1.5f;
    public float delayBeforeFadeOut = 2f;

    private void Start()
    {
        if (text != null)
        {
            text.alpha = 0f; // Texte transparent au départ
            StartCoroutine(FadeInOut());
        }
    }

    private IEnumerator FadeInOut()
    {
        while (true) // Boucle infinie
        {
            // Fade In (apparition)
            float timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                float progress = Mathf.SmoothStep(0f, 1f, timer / fadeDuration); // Easing
                text.alpha = progress;
                yield return null;
            }

            yield return new WaitForSeconds(delayBeforeFadeOut); // Pause

            // Fade Out (disparition)
            timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                float progress = Mathf.SmoothStep(1f, 0f, timer / fadeDuration); // Easing inverse
                text.alpha = progress;
                yield return null;
            }
        }
    }
}