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
        [TextArea(3, 10)] public string text;
        public float displayDuration = 2f;
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
    private bool skipRequested = false;

    void Start() {
        foreach (var go in objectsToEnableAfter) {
            if (go != null) go.SetActive(false);
        }

        if (slides == null || slides.Count == 0) {
            Debug.LogWarning("No slides assigned to CinematicPlayer!");
            return;
        }

        StartCoroutine(PlaySlide());
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)) {
            skipRequested = true;
        }
    }

    IEnumerator PlaySlide() {
        typing = true;
        readyForNext = false;
        skipRequested = false;

        yield return StartCoroutine(Fade(1));

        if (slides[index].image != null) {
            imageHolder.sprite = slides[index].image;
            imageHolder.color = Color.white;
            imageHolder.enabled = true;
        } else {
            imageHolder.sprite = null;
            imageHolder.color = Color.black;
            imageHolder.enabled = true;
        }

        textHolder.text = "";

        yield return StartCoroutine(Fade(0));

        foreach (char c in slides[index].text) {
            if (skipRequested) break;
            textHolder.text += c;
            yield return new WaitForSeconds(textSpeed);
        }

        textHolder.text = slides[index].text;
        typing = false;
        readyForNext = true;
        skipRequested = false;

        float timer = 0f;
        float waitTime = slides[index].displayDuration;

        while (timer < waitTime) {
            if (skipRequested) break;
            timer += Time.deltaTime;
            yield return null;
        }

        skipRequested = false;
        readyForNext = false;
        index++;

        if (index < slides.Count) {
            StartCoroutine(PlaySlide());
        } else {
            StartCoroutine(FadeOutCinematic());
        }
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

        foreach (var go in objectsToEnableAfter) {
            if (go != null) go.SetActive(true);
        }

        GameManager.Instance.OnFinishCinematic();
    }
}
