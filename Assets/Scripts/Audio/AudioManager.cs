using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    [Header("Mixer Groups")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("Sliders")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider uiSlider;

    [Header("UI Elements")]
    [SerializeField] private Button applyButton;
    [SerializeField] private Button cancelButton;

    private const string MASTER_VOL = "MasterVolume";
    private const string MUSIC_VOL = "MusicVolume";
    private const string SFX_VOL = "SFXVolume";
    private const string UI_VOL = "UIVolume";

    // Valeurs temporaires non sauvegardées
    private float pendingMasterVol;
    private float pendingMusicVol;
    private float pendingSFXVol;
    private float pendingUIVol;
    private bool hasUnsavedChanges = false;

    private void Awake()
    {
        if (!audioMixer)
        {
            Debug.LogError("AudioMixer non assigné dans AudioManager!");
            return;
        }
    }

    private void Start()
    {
        InitializeSliders();
        LoadVolumes();
        InitializeButtons();

        // Initialiser les valeurs temporaires
        pendingMasterVol = PlayerPrefs.GetFloat(MASTER_VOL, 0.8f);
        pendingMusicVol = PlayerPrefs.GetFloat(MUSIC_VOL, 0.7f);
        pendingSFXVol = PlayerPrefs.GetFloat(SFX_VOL, 0.9f);
        pendingUIVol = PlayerPrefs.GetFloat(UI_VOL, 0.9f);
    }

    private void InitializeSliders()
    {
        if (masterSlider)
        {
            masterSlider.onValueChanged.AddListener(value => {
                pendingMasterVol = value;
                SetVolume(MASTER_VOL, value, false);
                SetUnsavedChanges(true);
            });
        }

        if (musicSlider)
        {
            musicSlider.onValueChanged.AddListener(value => {
                pendingMusicVol = value;
                SetVolume(MUSIC_VOL, value, false);
                SetUnsavedChanges(true);
            });
        }

        if (sfxSlider)
        {
            sfxSlider.onValueChanged.AddListener(value => {
                pendingSFXVol = value;
                SetVolume(SFX_VOL, value, false);
                SetUnsavedChanges(true);
            });
        }

        if (uiSlider)
        {
            uiSlider.onValueChanged.AddListener(value => {
                pendingUIVol = value;
                SetVolume(UI_VOL, value, false);
                SetUnsavedChanges(true);
            });
        }
    }

    private void InitializeButtons()
    {
        if (applyButton)
        {
            applyButton.onClick.AddListener(ApplyAudioSettings);
            applyButton.interactable = false; // Désactivé par défaut
        }

        if (cancelButton)
        {
            cancelButton.onClick.AddListener(CancelPendingChanges);
            cancelButton.interactable = false; // Désactivé par défaut
        }
    }

    private void SetVolume(string parameterName, float value, bool save = true)
    {
        float volumeValue = Mathf.Clamp(value, 0.0001f, 1f);
        float dB = volumeValue <= 0.0001f ? -80f : Mathf.Log10(volumeValue) * 20f;
        
        audioMixer.SetFloat(parameterName, dB);
        
        if (save)
        {
            PlayerPrefs.SetFloat(parameterName, volumeValue);
        }
    }

    private void LoadVolumes()
    {
        float masterVol = PlayerPrefs.GetFloat(MASTER_VOL, 0.8f);
        float musicVol = PlayerPrefs.GetFloat(MUSIC_VOL, 0.7f);
        float sfxVol = PlayerPrefs.GetFloat(SFX_VOL, 0.9f);
        float uiVol = PlayerPrefs.GetFloat(UI_VOL, 0.9f);

        SetVolume(MASTER_VOL, masterVol);
        SetVolume(MUSIC_VOL, musicVol);
        SetVolume(SFX_VOL, sfxVol);
        SetVolume(UI_VOL, uiVol);

        if (masterSlider) masterSlider.value = masterVol;
        if (musicSlider) musicSlider.value = musicVol;
        if (sfxSlider) sfxSlider.value = sfxVol;
        if (uiSlider) uiSlider.value = uiVol;
    }

    public void ApplyAudioSettings()
    {
        SetVolume(MASTER_VOL, pendingMasterVol);
        SetVolume(MUSIC_VOL, pendingMusicVol);
        SetVolume(SFX_VOL, pendingSFXVol);
        SetVolume(UI_VOL, pendingUIVol);
        
        PlayerPrefs.Save();
        SetUnsavedChanges(false);
        Debug.Log("Paramètres audio sauvegardés");
    }

    public void CancelPendingChanges()
    {
        // Restaure les dernières valeurs sauvegardées
        float masterVol = PlayerPrefs.GetFloat(MASTER_VOL, 0.8f);
        float musicVol = PlayerPrefs.GetFloat(MUSIC_VOL, 0.7f);
        float sfxVol = PlayerPrefs.GetFloat(SFX_VOL, 0.9f);
        float uiVol = PlayerPrefs.GetFloat(UI_VOL, 0.9f);

        // Met à jour les sliders et les valeurs temporaires
        if (masterSlider) masterSlider.value = masterVol;
        if (musicSlider) musicSlider.value = musicVol;
        if (sfxSlider) sfxSlider.value = sfxVol;
        if (uiSlider) uiSlider.value = uiVol;

        pendingMasterVol = masterVol;
        pendingMusicVol = musicVol;
        pendingSFXVol = sfxVol;
        pendingUIVol = uiVol;

        // Applique les volumes (sans sauvegarder)
        SetVolume(MASTER_VOL, masterVol, false);
        SetVolume(MUSIC_VOL, musicVol, false);
        SetVolume(SFX_VOL, sfxVol, false);
        SetVolume(UI_VOL, uiVol, false);

        SetUnsavedChanges(false);
    }

    public void ResetToDefault()
    {
        // Valeurs par défaut
        float defaultMaster = 0.8f;
        float defaultMusic = 0.7f;
        float defaultSFX = 0.9f;
        float defaultUI = 0.9f;

        // Met à jour les sliders et les valeurs temporaires
        if (masterSlider) masterSlider.value = defaultMaster;
        if (musicSlider) musicSlider.value = defaultMusic;
        if (sfxSlider) sfxSlider.value = defaultSFX;
        if (uiSlider) uiSlider.value = defaultUI;

        pendingMasterVol = defaultMaster;
        pendingMusicVol = defaultMusic;
        pendingSFXVol = defaultSFX;
        pendingUIVol = defaultUI;

        // Applique les volumes (sans sauvegarder)
        SetVolume(MASTER_VOL, defaultMaster, false);
        SetVolume(MUSIC_VOL, defaultMusic, false);
        SetVolume(SFX_VOL, defaultSFX, false);
        SetVolume(UI_VOL, defaultUI, false);

        SetUnsavedChanges(true);
    }

    private void SetUnsavedChanges(bool state)
    {
        hasUnsavedChanges = state;
        
        // Active/désactive les boutons selon l'état
        if (applyButton) applyButton.interactable = state;
        if (cancelButton) cancelButton.interactable = state;
    }

    private void OnDestroy()
    {
        if (masterSlider) masterSlider.onValueChanged.RemoveAllListeners();
        if (musicSlider) musicSlider.onValueChanged.RemoveAllListeners();
        if (sfxSlider) sfxSlider.onValueChanged.RemoveAllListeners();
        if (uiSlider) uiSlider.onValueChanged.RemoveAllListeners();

        if (applyButton) applyButton.onClick.RemoveAllListeners();
        if (cancelButton) cancelButton.onClick.RemoveAllListeners();
    }
}