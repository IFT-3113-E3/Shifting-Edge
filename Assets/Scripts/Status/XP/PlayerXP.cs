using UnityEngine;
using System;

public class PlayerXP : MonoBehaviour
{
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int currentXP = 0;
    
    // Événements pour les notifications
    public event Action<int> OnLevelUp; // Déclenché lorsque le joueur monte de niveau
    public event Action<int> OnXPGained; // Déclenché lorsque le joueur gagne de l'XP
    public event Action<int, int> OnXPChanged; // Déclenché lorsque l'XP est mise à jour (valeur actuelle, valeur requise pour le prochain niveau)
    
    // Propriétés publiques pour accéder aux valeurs
    public int CurrentLevel => currentLevel;
    public int CurrentXP => currentXP;
    
    // XP restante nécessaire pour le prochain niveau
    public int XPToNextLevel
    {
        get { return XPManager.Instance.GetRemainingXPForNextLevel(currentLevel, currentXP); }
    }
    
    // Pour les effets de niveau
    [SerializeField] private GameObject levelUpEffectPrefab;
    
    private void Start()
    {
        // S'assurer que l'XP et le niveau sont cohérents au démarrage
        UpdateLevelBasedOnXP();
        
        // Notifier les UI au démarrage
        OnXPChanged?.Invoke(currentXP, XPToNextLevel);
    }
    
    // Ajouter de l'XP au joueur
    public void AddXP(int amount)
    {
        if (amount <= 0) return;
        
        int oldLevel = currentLevel;
        currentXP += amount;
        
        // Vérifier si le joueur a monté de niveau
        UpdateLevelBasedOnXP();
        
        // Déclencher les événements
        OnXPGained?.Invoke(amount);
        OnXPChanged?.Invoke(currentXP, XPToNextLevel);
        
        if (currentLevel > oldLevel)
        {
            // Le joueur a monté de niveau
            for (int i = oldLevel + 1; i <= currentLevel; i++)
            {
                OnLevelUp?.Invoke(i);
                PlayLevelUpEffect();
            }
        }
    }
    
    // Mettre à jour le niveau en fonction de l'XP actuelle
    private void UpdateLevelBasedOnXP()
    {
        int newLevel = XPManager.Instance.GetLevelFromXP(currentXP);
        currentLevel = newLevel;
    }
    
    // Définir manuellement le niveau et l'XP (utile pour le chargement de sauvegarde)
    public void SetLevelAndXP(int level, int xp)
    {
        currentLevel = Mathf.Max(1, level);
        currentXP = Mathf.Max(0, xp);
        OnXPChanged?.Invoke(currentXP, XPToNextLevel);
    }
    
    private void PlayLevelUpEffect()
    {
        ItemData reward = RewardSystem.Instance.GetRewardForLevel(currentLevel, out int quantity);
        if (reward != null)
        {
            // Ajoute le mana au PlayerInventory plutôt qu'à un compteur local
            PlayerInventory.Instance.AddMana(quantity);
            Debug.Log($"Reçu {quantity} SkillTreeMana (Total: {PlayerInventory.Instance.SkillTreeMana})");
        }
    }
}