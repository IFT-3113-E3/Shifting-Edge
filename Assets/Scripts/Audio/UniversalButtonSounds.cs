using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
public class UniversalButtonSounds : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [Header("Audio")]
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioMixerGroup menuMixerGroup; // Assignez votre groupe "menus"

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.outputAudioMixerGroup = menuMixerGroup; // Lie le mixer
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverSound != null && GetComponent<Button>().interactable)
            audioSource.PlayOneShot(hoverSound);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (clickSound != null && GetComponent<Button>().interactable)
            audioSource.PlayOneShot(clickSound);
    }
}