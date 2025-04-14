using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class SaveSlotManager : MonoBehaviour
{
    public GameObject slotPrefab; // Prefab du slot de sauvegarde
    public Transform slotsParent; // Parent pour organiser les slots
    public SaveData[] saveSlots = new SaveData[3]; // Tableau de sauvegardes

    void Start()
    {
        // Charger les données de sauvegarde (à implémenter)
        LoadSaveData();

        // Créer les slots
        for (int i = 0; i < saveSlots.Length; i++)
        {
            GameObject slot = Instantiate(slotPrefab, slotsParent);
            ConfigureSlot(slot, saveSlots[i], i);
        }
    }

    void ConfigureSlot(GameObject slot, SaveData data, int slotIndex)
    {
        // Récupérer les composants UI
        Text chapterText = slot.transform.Find("ChapterText").GetComponent<Text>();
        Text timeText = slot.transform.Find("TimeText").GetComponent<Text>();
        Text levelText = slot.transform.Find("LevelText").GetComponent<Text>();
        Text dateText = slot.transform.Find("DateText").GetComponent<Text>();
        Button button = slot.GetComponent<Button>();

        // Afficher les données ou "Vide"
        if (data.isUsed)
        {
            chapterText.text = "Chapitre : " + data.chapterName;
            timeText.text = "Temps joué : " + FormatTime(data.playTime);
            levelText.text = "Niveau : " + data.levelReached;
            dateText.text = "Dernière sauvegarde : " + data.saveDate;
        }
        else
        {
            chapterText.text = "Vide";
            timeText.text = "";
            levelText.text = "";
            dateText.text = "";
        }

        // Gérer le survol
        EventTrigger trigger = slot.GetComponent<EventTrigger>();
        if (trigger == null) trigger = slot.AddComponent<EventTrigger>();

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerEnter;
        entry.callback.AddListener((eventData) => { OnSlotHover(slot, data); });
        trigger.triggers.Add(entry);

        // Gérer le clic
        button.onClick.AddListener(() => OnSlotClicked(slotIndex));
    }

    void OnSlotHover(GameObject slot, SaveData data)
    {
        if (!data.isUsed)
        {
            Text chapterText = slot.transform.Find("ChapterText").GetComponent<Text>();
            chapterText.text = "Nouvelle partie";
        }
    }

    void OnSlotClicked(int slotIndex)
    {
        Debug.Log("Slot cliqué : " + slotIndex);
        // Charger ou démarrer une nouvelle partie
    }

    string FormatTime(float seconds)
    {
        TimeSpan time = TimeSpan.FromSeconds(seconds);
        return string.Format("{0:D2}:{1:D2}:{2:D2}", time.Hours, time.Minutes, time.Seconds);
    }

    void LoadSaveData()
    {
        // Charger les données de sauvegarde (à implémenter)
        // Exemple de données pour tester :
        saveSlots[0] = new SaveData
        {
            chapterName = "Chapitre 1",
            playTime = 3600, // 1 heure
            levelReached = 5,
            saveDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
            isUsed = true
        };
        saveSlots[1] = new SaveData { isUsed = false }; // Slot vide
        saveSlots[2] = new SaveData { isUsed = false }; // Slot vide
    }
}