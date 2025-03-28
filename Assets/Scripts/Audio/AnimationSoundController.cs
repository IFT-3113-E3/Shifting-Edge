using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AnimationSoundController : MonoBehaviour
{
    [System.Serializable]
    public class SoundMapping
    {
        public string animationEventName;
        public AudioClip clip;
        [Range(0.1f, 2f)] public float pitchVariation = 1f;
    }

    [SerializeField] private SoundMapping[] soundLibrary;
    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    // À appeler depuis les Animation Events
    public void PlayAnimationSound(string eventName)
    {
        var sound = System.Array.Find(soundLibrary, s => s.animationEventName == eventName);
        if (sound == null) return;

        _audioSource.pitch = Random.Range(1f/sound.pitchVariation, sound.pitchVariation);
        _audioSource.PlayOneShot(sound.clip);
    }
}