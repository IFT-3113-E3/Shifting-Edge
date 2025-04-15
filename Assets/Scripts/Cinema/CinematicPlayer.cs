using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

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

    private int index = 0;
    private bool typing = false;
    private bool readyForNext = false;

    void Start() {
        StartCoroutine(PlaySlide());
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
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
                    StartCoroutine(FadeToBlackThenLoad("MainScene"));
                }
            }
        }
    }

    IEnumerator PlaySlide() {
        typing = true;
        readyForNext = false;

        // Fade to black
        yield return StartCoroutine(Fade(1));

        // Change image
        imageHolder.sprite = slides[index].image;

        // Fade in
        yield return StartCoroutine(Fade(0));

        // Display text
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
            t += Time.deltaTime;
            fadeOverlay.alpha = Mathf.Lerp(start, targetAlpha, t / fadeDuration);
            yield return null;
        }
        fadeOverlay.alpha = targetAlpha;
    }

    IEnumerator FadeToBlackThenLoad(string sceneName) {
        yield return StartCoroutine(Fade(1));
        SceneManager.LoadScene(sceneName);
    }
}
