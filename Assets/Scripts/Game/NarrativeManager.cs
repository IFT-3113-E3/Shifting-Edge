using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NarrativeManager : MonoBehaviour
{
    public static NarrativeManager Instance;

    [Header("UI Références")]
    public CanvasGroup dialogueBox;
    public TextMeshProUGUI dialogueText;
    public CanvasGroup chapterBanner;
    public TextMeshProUGUI chapterText;
    public TextMeshProUGUI areaText;

    [Header("Réglages")]
    public float textDisplayDelay = 0.05f;
    public float minLineDuration = 0.5f;
    public KeyCode[] advanceKeys = { KeyCode.Space, KeyCode.Mouse0 };
    
    private bool _isRunning = false;
    private bool _canAdvance = false;

    private void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        else Instance = this;

        dialogueBox.alpha = 0;
        dialogueBox.gameObject.SetActive(false);

        chapterBanner.alpha = 0;
        chapterBanner.gameObject.SetActive(false);
    }

    public void PlayEvent(StoryEvent storyEvent)
    {
        if (_isRunning) return;
        StartCoroutine(PlayNarrative(storyEvent));
    }

    private IEnumerator PlayNarrative(StoryEvent story)
    {
        _isRunning = true;

        // Lock player si demandé
        var player = FindObjectOfType<PlayerController>();
        if (story.lockPlayerDuringDialogue && player != null)
            player.enabled = false;

        // Affichage Chapitre
        if (story.showChapterBanner)
        {
            chapterBanner.gameObject.SetActive(true);
            chapterText.text = story.chapterTitle;
            areaText.text = story.areaName;
            yield return StartCoroutine(FadeCanvas(chapterBanner, true));
            yield return new WaitForSeconds(2.5f);
            yield return StartCoroutine(FadeCanvas(chapterBanner, false));
            chapterBanner.gameObject.SetActive(false);
        }

        // Dialogue
        dialogueBox.gameObject.SetActive(true);
        yield return StartCoroutine(FadeCanvas(dialogueBox, true));

        foreach (var line in story.dialogueLines)
        {
            dialogueText.text = "";
            _canAdvance = false;

            // Affichage lettre par lettre
            foreach (var c in line)
            {
                dialogueText.text += c;
                yield return new WaitForSeconds(textDisplayDelay);
            }

            yield return new WaitForSeconds(minLineDuration);
            _canAdvance = true;

            // Attente d'une entrée utilisateur
            yield return new WaitUntil(() => _canAdvance && AnyAdvanceInput());
        }

        yield return StartCoroutine(FadeCanvas(dialogueBox, false));
        dialogueBox.gameObject.SetActive(false);

        // Unlock player
        if (story.lockPlayerDuringDialogue && player != null)
            player.enabled = true;

        _isRunning = false;
    }

    private bool AnyAdvanceInput()
    {
        foreach (var key in advanceKeys)
        {
            if (Input.GetKeyDown(key)) return true;
        }
        return false;
    }

    private IEnumerator FadeCanvas(CanvasGroup cg, bool fadeIn, float duration = 0.5f)
    {
        float t = 0f;
        float start = cg.alpha;
        float end = fadeIn ? 1f : 0f;

        while (t < duration)
        {
            cg.alpha = Mathf.Lerp(start, end, t / duration);
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        cg.alpha = end;
    }
}
