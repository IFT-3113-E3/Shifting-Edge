using TMPro;
using UnityEngine;

public class InteractionUI : MonoBehaviour
{
    public static InteractionUI Instance;
    public TextMeshProUGUI interactionText;

    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;

        interactionText.text = "";
    }

    public void ShowText(string message)
    {
        interactionText.text = message;
    }

    public void HideText()
    {
        interactionText.text = "";
    }
}
