using UnityEngine;
using UnityEngine.Audio;

namespace Audio
{
    public class AudioSingleton : MonoBehaviour
    {
        private static AudioSingleton instance;
        
        public static AudioSingleton Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<AudioSingleton>();
                    if (instance == null)
                    {
                        GameObject singletonObject = new GameObject(nameof(AudioSingleton));
                        instance = singletonObject.AddComponent<AudioSingleton>();
                        DontDestroyOnLoad(singletonObject);
                    }
                }
                return instance;
            }
        }
        
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioMixer audioMixer;
        
        public AudioMixer AudioMixer => audioMixer;
        
        public AudioMixerGroup SfxMixerGroup => audioMixer?.FindMatchingGroups("SFX")[0];
        public AudioMixerGroup MusicMixerGroup => audioMixer?.FindMatchingGroups("Music")[0];
        public AudioMixerGroup MenuMixerGroup => audioMixer?.FindMatchingGroups("UI")[0];

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}