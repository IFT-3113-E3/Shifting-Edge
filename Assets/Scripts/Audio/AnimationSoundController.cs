using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class AnimationSoundController : MonoBehaviour
{
    [System.Serializable]
    public class SoundGroup
    {
        public string eventName;
        public AudioClip[] clips;
        [Range(0.8f, 1.2f)] public float pitchVariation = 1.1f;
        public bool noRepeat = true;
    }

    [Header("Sound Settings")]
    [SerializeField] private SoundGroup[] soundGroups;

    private AudioSource _audioSource;
    private Dictionary<string, List<AudioClip>> _availableClips = new Dictionary<string, List<AudioClip>>();

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        InitializeAvailableClips();
    }

    private void InitializeAvailableClips()
    {
        foreach (var group in soundGroups)
        {
            _availableClips[group.eventName] = new List<AudioClip>(group.clips);
        }
    }

    // Méthode appelée par les Animation Events
    public void PlayRandomSound(string eventName)
    {
        if (!_availableClips.ContainsKey(eventName)) return;

        var group = System.Array.Find(soundGroups, g => g.eventName == eventName);
        var availableClips = _availableClips[eventName];

        if (availableClips.Count == 0 && group.noRepeat)
        {
            RefreshAvailableClips(eventName);
            availableClips = _availableClips[eventName];
        }

        if (availableClips.Count == 0) return;

        var randomIndex = Random.Range(0, availableClips.Count);
        var selectedClip = availableClips[randomIndex];

        _audioSource.pitch = Random.Range(1f / group.pitchVariation, group.pitchVariation);
        _audioSource.PlayOneShot(selectedClip);

        if (group.noRepeat)
        {
            availableClips.RemoveAt(randomIndex);
        }
    }

    private void RefreshAvailableClips(string eventName)
    {
        var group = System.Array.Find(soundGroups, g => g.eventName == eventName);
        if (group != null)
        {
            _availableClips[eventName] = new List<AudioClip>(group.clips);
        }
    }

    // Méthodes spécifiques pour faciliter l'assignation dans les Animation Events
    public void PlayRandomFootstep() => PlayRandomSound("Footstep");
    public void PlayRandomSwing() => PlayRandomSound("Swing");
    public void PlayRandomJump() => PlayRandomSound("Jump");
    public void PlayRandomLand() => PlayRandomSound("Land");
}