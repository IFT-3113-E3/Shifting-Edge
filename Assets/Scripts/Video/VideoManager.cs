using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

namespace UI
{
    public class VideoManager : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TMP_Dropdown resolutionDropdown;
        [SerializeField] private TMP_Dropdown displayModeDropdown;
        [SerializeField] private Button defaultButton;

        [Header("Pixel Art Setup")]
        [SerializeField] private VirtualScreen virtualScreen;
        [SerializeField] private GameCanvasScaler canvasScaler;

        private List<Resolution> availableResolutions = new List<Resolution>();
        private Resolution pendingResolution;
        private FullScreenMode pendingDisplayMode;
        private bool hasUnsavedChanges;

        private void Start()
        {
            foreach (var dropdown in FindObjectsByType<TMP_Dropdown>(FindObjectsSortMode.InstanceID))
            {
                var template = dropdown.template?.GetComponent<VirtualScreen>();
                if (template != null) template.enabled = false;
            }

            FixAllDropdowns();

            InitializeResolutionOptions();
            InitializeDisplayModeOptions();
            LoadCurrentSettings();
            InitializeButtons();
        }

        private void FixAllDropdowns()
    {
        foreach (var dropdown in FindObjectsByType<TMP_Dropdown>(FindObjectsSortMode.None))
        {
            if (dropdown.template != null)
            {
                var vs = dropdown.template.GetComponent<VirtualScreen>();
                if (vs != null) vs.enabled = false;
            }
        }
    }

        private void InitializeResolutionOptions()
        {
            if (!resolutionDropdown) return;

            Resolution[] resolutions = Screen.resolutions;
            availableResolutions.Clear();
            resolutionDropdown.ClearOptions();

            var uniqueResolutions = new Dictionary<string, Resolution>();
            foreach (var res in resolutions)
            {
                string key = $"{res.width}x{res.height}";
                if (!uniqueResolutions.ContainsKey(key))
                {
                    uniqueResolutions.Add(key, res);
                    availableResolutions.Add(res);
                }
            }

            var options = new List<string>();
            int currentIndex = 0;
            Resolution currentResolution = Screen.currentResolution;

            for (int i = 0; i < availableResolutions.Count; i++)
            {
                options.Add($"{availableResolutions[i].width}x{availableResolutions[i].height}");

                if (availableResolutions[i].width == currentResolution.width &&
                    availableResolutions[i].height == currentResolution.height)
                {
                    currentIndex = i;
                }
            }

            resolutionDropdown.AddOptions(options);
            resolutionDropdown.value = currentIndex;
            resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        }

        private void InitializeDisplayModeOptions()
        {
            if (!displayModeDropdown) return;

            displayModeDropdown.ClearOptions();
            displayModeDropdown.AddOptions(new List<string>{"Plein écran", "Fenêtré", "Plein écran fenêtré"});
            displayModeDropdown.value = (int)Screen.fullScreenMode;
            displayModeDropdown.onValueChanged.AddListener(OnDisplayModeChanged);
        }

        private void LoadCurrentSettings()
        {
            pendingResolution = Screen.currentResolution;
            pendingDisplayMode = Screen.fullScreenMode;
        }

        private void InitializeButtons()
        {
            if (defaultButton) defaultButton.onClick.AddListener(CancelPendingChanges);
            UpdateButtonStates();
        }

        private void OnResolutionChanged(int index)
        {
            if (index >= 0 && index < availableResolutions.Count)
            {
                pendingResolution = availableResolutions[index];
                SetUnsavedChanges(true);
            }
        }

        private void OnDisplayModeChanged(int index)
        {
            pendingDisplayMode = (FullScreenMode)index;
            SetUnsavedChanges(true);
        }

        public void ApplyVideoSettings()
        {
            Screen.SetResolution(pendingResolution.width, pendingResolution.height, pendingDisplayMode);

            // Remplacez cette partie :
            if (canvasScaler != null)
            {
                VideoSettingsHelper.RefreshPixelPerfectRendering(canvasScaler);
            }

            PlayerPrefs.SetInt("ScreenWidth", pendingResolution.width);
            PlayerPrefs.SetInt("ScreenHeight", pendingResolution.height);
            PlayerPrefs.SetInt("FullScreenMode", (int)pendingDisplayMode);
            PlayerPrefs.Save();

            SetUnsavedChanges(false);
        }

        public void CancelPendingChanges()
        {
            resolutionDropdown.value = availableResolutions.FindIndex(
                r => r.width == Screen.currentResolution.width &&
                     r.height == Screen.currentResolution.height);

            displayModeDropdown.value = (int)Screen.fullScreenMode;

            pendingResolution = Screen.currentResolution;
            pendingDisplayMode = Screen.fullScreenMode;

            SetUnsavedChanges(false);
        }

        private void SetUnsavedChanges(bool state)
        {
            hasUnsavedChanges = state;
            UpdateButtonStates();
        }

        private void UpdateButtonStates()
        {
            if (defaultButton) defaultButton.interactable = hasUnsavedChanges;
        }

        private void OnDestroy()
        {
            if (resolutionDropdown) resolutionDropdown.onValueChanged.RemoveListener(OnResolutionChanged);
            if (displayModeDropdown) displayModeDropdown.onValueChanged.RemoveListener(OnDisplayModeChanged);
        }
    }
}