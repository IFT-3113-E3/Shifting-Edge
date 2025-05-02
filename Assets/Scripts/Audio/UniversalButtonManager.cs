using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using TMPro;

[RequireComponent(typeof(Button))]
public class UniversalButtonManager : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
{
    [Header("Audio")]
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioMixerGroup menuMixerGroup;
    private AudioSource audioSource;

    [Header("Visual")]
    [SerializeField] private TMP_Text buttonText;
    [SerializeField] private Color originalTextColor;

    [SerializeField] public float hoverTransparency = 0.5f;
    [SerializeField] public float transitionSpeed = 0.2f;


    private void Awake()
    {
        buttonText = GetComponentInChildren<TMP_Text>();
        originalTextColor = buttonText.color;

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.outputAudioMixerGroup = menuMixerGroup;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(FadeToTransparency(hoverTransparency));

        if (hoverSound != null && GetComponent<Button>().interactable)
            audioSource.PlayOneShot(hoverSound);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(FadeToTransparency(1f));
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (clickSound != null && GetComponent<Button>().interactable)
            audioSource.PlayOneShot(clickSound);
    }

    private IEnumerator FadeToTransparency(float targetAlpha)
    {
        float currentTextAlpha = buttonText != null ? buttonText.color.a : 1f;
        float time = 0;

        // Easing
        while (time < transitionSpeed)
        {
            time += Time.unscaledDeltaTime;

            float newTextAlpha = Mathf.Lerp(currentTextAlpha, targetAlpha, time / transitionSpeed);
            buttonText.color = new Color(originalTextColor.r, originalTextColor.g, originalTextColor.b, newTextAlpha);

            yield return new WaitForEndOfFrame();
        }

        buttonText.color = new Color(originalTextColor.r, originalTextColor.g, originalTextColor.b, targetAlpha);
    }
    
}