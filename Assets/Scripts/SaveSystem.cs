using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private const int MaxSaveSlots = 5;
    
    private static string GetSavePath(int slot)
    {
        return Path.Combine(Application.persistentDataPath, $"save_{slot}.json");
    }
    
    public static void DeleteSave(int slot)
    {
        var savePath = GetSavePath(slot);
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
        }
    }
    
    public static bool SaveExists(int slot) => File.Exists(GetSavePath(slot));
    
    public static bool GetNextSaveSlot(out int slot)
    {
        for (slot = 0; slot < MaxSaveSlots; slot++)
        {
            if (!SaveExists(slot))
            {
                return true;
            }
        }

        slot = -1;
        return false;
    }
    
    public static void SaveGameSession(GameSession session, int slot)
    {
        if (session == null)
        {
            Debug.LogError("Cannot save a null game session.");
            return;
        }
        
        if (slot is < 0 or >= MaxSaveSlots)
        {
            Debug.LogError($"Invalid save slot: {slot}. Must be between 0 and {MaxSaveSlots - 1}.");
            return;
        }
        
        var data = new SessionSaveData
        {
            worldSectionId = session.spawnPointId,
            spawnPointId = session.spawnPointId
        };
        session.PlayerStats.SaveData(ref data);

        var json = JsonUtility.ToJson(data, prettyPrint: true);
        File.WriteAllText(GetSavePath(slot), json);
    }

    public static SessionSaveData LoadSaveData(int slot)
    {
        var savePath = GetSavePath(slot);
        if (!File.Exists(savePath))
            return null;

        var json = File.ReadAllText(savePath);
        return JsonUtility.FromJson<SessionSaveData>(json);
    }
}