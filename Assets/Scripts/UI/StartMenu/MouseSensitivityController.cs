using UnityEngine;
using UnityEngine.UI;

public class MouseSensitivityController : MonoBehaviour
{
    [Header("Références")]
    public Slider sensitivitySlider;

    [Header("Paramètres")]
    public float minSensitivity = 0.1f;
    public float maxSensitivity = 10f;
    public float defaultValue = 2f;

    public static float CurrentMouseSensitivity { get; private set; }

    private void Start()
    {
        sensitivitySlider.minValue = minSensitivity;
        sensitivitySlider.maxValue = maxSensitivity;
        
        float savedSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", defaultValue);
        sensitivitySlider.value = savedSensitivity;
        UpdateSensitivity(savedSensitivity);

        sensitivitySlider.onValueChanged.AddListener(UpdateSensitivity);
    }

    private void UpdateSensitivity(float newValue)
    {
        CurrentMouseSensitivity = newValue;
        
        PlayerPrefs.SetFloat("MouseSensitivity", newValue);
        PlayerPrefs.Save();
    }

    public void ResetToDefault()
    {
        sensitivitySlider.value = defaultValue;
    }
}