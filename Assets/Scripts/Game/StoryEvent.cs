using UnityEngine;

[CreateAssetMenu(menuName = "Narrative/Story Event")]
public class StoryEvent : ScriptableObject
{
    [Header("ID unique pour la sauvegarde")]
    public string eventID;

    [Header("Chapitre et zone (affiché en grand à l'écran)")]
    public string chapterTitle;
    public string areaName;

    [Header("Lignes de dialogue")]
    [TextArea(2, 4)] public string[] dialogueLines;

    [Header("Contrôle joueur")]
    public bool lockPlayerDuringDialogue = false;

    [Header("Affichage de l'entête de chapitre")]
    public bool showChapterBanner = true;
}
