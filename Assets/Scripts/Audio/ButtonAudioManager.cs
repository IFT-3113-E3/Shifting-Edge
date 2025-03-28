using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class ButtonSoundManager : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [Header("Audio Clips")]
    public AudioClip hoverSound;
    public AudioClip clickSound;

    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        
        // S'abonne aux événements de tous les boutons
        Button[] allButtons = FindObjectsOfType<Button>(true);
        foreach (Button btn in allButtons)
        {
            AddTriggers(btn.gameObject);
        }
    }

    private void AddTriggers(GameObject buttonObj)
    {
        // Ajoute ce script comme composant aux boutons s'il n'existe pas déjà
        if (!buttonObj.GetComponent<ButtonSoundManager>())
        {
            buttonObj.AddComponent<ButtonSoundManager>();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverSound != null)
            _audioSource.PlayOneShot(hoverSound);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (clickSound != null)
            _audioSource.PlayOneShot(clickSound);
    }
}