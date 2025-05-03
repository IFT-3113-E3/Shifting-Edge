using System.Collections.Generic;
using UnityEngine;
using System;

public class XPManager : MonoBehaviour
{
    public static XPManager Instance { get; private set; }

    [Serializable]
    public class LevelData
    {
        public int level;
        public int requiredXP;
        public int totalXPToReachLevel; // XP cumulée nécessaire pour atteindre ce niveau
    }

    [SerializeField]
    private List<LevelData> levelProgressionData = new List<LevelData>();

    [SerializeField]
    private int maxLevel = 100;
    private float xpMultiplier = 1f;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Initialiser ou valider la progression des niveaux
        InitializeLevelData();
    }

    private void InitializeLevelData()
    {
        // Si aucune donnée de progression n'est définie, créer une formule par défaut
        if (levelProgressionData.Count == 0)
        {
            int runningTotal = 0;
            for (int level = 1; level <= maxLevel; level++)
            {
                int xpForThisLevel = CalculateXPForLevel(level);
                runningTotal += xpForThisLevel;

                LevelData levelData = new LevelData
                {
                    level = level,
                    requiredXP = xpForThisLevel,
                    totalXPToReachLevel = runningTotal
                };

                levelProgressionData.Add(levelData);
            }
        }
        else
        {
            // S'assurer que les totaux cumulés sont corrects
            int runningTotal = 0;
            for (int i = 0; i < levelProgressionData.Count; i++)
            {
                runningTotal += levelProgressionData[i].requiredXP;
                levelProgressionData[i].totalXPToReachLevel = runningTotal;
            }
        }
    }

    public void AddXPMultiplier(float multiplier)
    {
        xpMultiplier += multiplier;
        Debug.Log($"Multiplicateur d'XP mis à jour: {xpMultiplier}x");
    }

    public float GetCurrentXPMultiplier()
    {
        return xpMultiplier;
    }

    public void ResetXPBonuses()
    {
        xpMultiplier = 1f;
    }

    // Formule pour calculer l'XP requise pour un niveau
    private int CalculateXPForLevel(int level)
    {
        // Exemple de formule: 50 * (level^1.5)
        // Vous pouvez ajuster cette formule selon vos besoins de progression
        return (int)(50 * Mathf.Pow(level, 1.5f));
    }

    public int CalculateBoostedXP(int baseXP)
    {
        return Mathf.RoundToInt(baseXP * xpMultiplier);
    }

    // Obtenir le niveau correspondant à un montant d'XP total
    public int GetLevelFromXP(int totalXP)
    {
        int level = 1; // Niveau par défaut (si totalXP < 100)
        
        for (int i = 0; i < levelProgressionData.Count; i++)
        {
            if (totalXP >= levelProgressionData[i].totalXPToReachLevel)
            {
                level = levelProgressionData[i].level;
            }
            else
            {
                break;
            }
        }
        
        return Mathf.Min(level, maxLevel);
    }
    
    // Nouvelle méthode: obtenir l'XP restante nécessaire pour passer au niveau suivant
    public int GetRemainingXPForNextLevel(int currentLevel, int currentXP)
    {
        if (currentLevel >= maxLevel) return 0; // Au niveau max
        
        // Obtenir le total d'XP requis pour le niveau suivant
        int nextLevelIndex = currentLevel;
        if (nextLevelIndex < levelProgressionData.Count)
        {
            int xpForNextLevel = levelProgressionData[nextLevelIndex].totalXPToReachLevel;
            return xpForNextLevel - currentXP;
        }
        
        return 0;
    }

    // Obtenir l'XP nécessaire pour passer au niveau suivant (total pour ce niveau uniquement)
    public int GetXPRequiredForNextLevel(int currentLevel)
    {
        if (currentLevel >= maxLevel) return 0; // Au niveau max

        int index = currentLevel - 1; // Niveaux commencent à 1, index à 0
        if (index >= 0 && index < levelProgressionData.Count)
        {
            return levelProgressionData[index].requiredXP;
        }
        return 0;
    }

    // Obtenir l'XP totale nécessaire pour atteindre un niveau
    public int GetTotalXPForLevel(int level)
    {
        int index = level - 1;
        if (index >= 0 && index < levelProgressionData.Count)
        {
            return levelProgressionData[index].totalXPToReachLevel;
        }
        return 0;
    }

    // Obtenir l'XP actuelle pour le niveau courant (depuis le début du niveau)
    public int GetCurrentLevelXP(int currentLevel, int totalXP)
    {
        int previousLevelTotalXP = 0;
        if (currentLevel > 1)
        {
            previousLevelTotalXP = levelProgressionData[currentLevel - 2].totalXPToReachLevel;
        }
        
        return totalXP - previousLevelTotalXP;
    }

    // Pour calculer le pourcentage de progression dans le niveau actuel
    public float GetLevelProgressPercentage(int currentLevel, int totalXP)
    {
        if (currentLevel >= maxLevel) return 1.0f;
        
        int currentLevelXP = GetCurrentLevelXP(currentLevel, totalXP);
        int requiredXP = GetXPRequiredForNextLevel(currentLevel);
        
        if (requiredXP <= 0) return 1.0f; // 100% si au niveau max
        return (float)currentLevelXP / requiredXP;
    }
}