using UnityEngine;
using UnityEngine.UI;
using Status;

public class CharacterMenuManager : MonoBehaviour
{
    public GameObject characterPanel;
    public Text hpText;
    public Text defText;
    public Text atkText;
    public Button returnButton;
    public GameObject pauseMenuPanel;

    public EntityStatus playerStatus;

    private void Start()
    {
        returnButton.onClick.AddListener(CloseAndReturn);
        RefreshStats();
    }

    private void OnEnable()
    {
        RefreshStats();
    }

    private void RefreshStats()
    {
        if (playerStatus != null)
        {
            hpText.text = "PV: " + playerStatus.CurrentHealth + "/" + playerStatus.maxHealth;
            defText.text = "DEF: ?";
            atkText.text = "ATK: ?";
        }
    }

    private void CloseAndReturn()
    {
        characterPanel.SetActive(false);
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);
    }
}
