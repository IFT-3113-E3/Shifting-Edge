using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CinematicPlayer : MonoBehaviour
{
    [System.Serializable]
    public class Slide {
        public Sprite image;
        [TextArea(3,10)] public string text;
    }

    public List<Slide> slides;
    public Image imageHolder;
    public TextMeshProUGUI textHolder;
    public CanvasGroup fadeOverlay;
    public float textSpeed = 0.03f;
    public float fadeDuration = 1.0f;

    public GameObject[] objectsToEnableAfter;

    private int index = 0;
    private bool typing = false;
    private bool readyForNext = false;

    void Start() {
        // Désactive tous les objets de gameplay
        foreach (var go in objectsToEnableAfter) go.SetActive(false);

        StartCoroutine(PlaySlide());
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)) {
            if (typing) {
                StopAllCoroutines();
                textHolder.text = slides[index].text;
                typing = false;
                readyForNext = true;
            } else if (readyForNext) {
                index++;
                if (index < slides.Count) {
                    StartCoroutine(PlaySlide());
                } else {
                    StartCoroutine(FadeOutCinematic());
                }
            }
        }
    }

    IEnumerator PlaySlide() {
        typing = true;
        readyForNext = false;

        yield return StartCoroutine(Fade(1));

        imageHolder.sprite = slides[index].image;

        yield return StartCoroutine(Fade(0));

        textHolder.text = "";
        foreach (char c in slides[index].text) {
            textHolder.text += c;
            yield return new WaitForSeconds(textSpeed);
        }

        typing = false;
        readyForNext = true;
    }

    IEnumerator Fade(float targetAlpha) {
        float t = 0;
        float start = fadeOverlay.alpha;
        while (t < fadeDuration) {
            t += Time.unscaledDeltaTime;
            fadeOverlay.alpha = Mathf.Lerp(start, targetAlpha, t / fadeDuration);
            yield return null;
        }
        fadeOverlay.alpha = targetAlpha;
    }

    IEnumerator FadeOutCinematic() {
        yield return StartCoroutine(Fade(1));
        imageHolder.enabled = false;
        textHolder.text = "";

        yield return StartCoroutine(Fade(0));

        foreach (var go in objectsToEnableAfter) go.SetActive(true);
        Destroy(gameObject); // Optionnel : auto-détruire la cinématique
    }
}
