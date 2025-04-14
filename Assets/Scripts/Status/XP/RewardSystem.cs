using System.Collections.Generic;
using UnityEngine;

public class RewardSystem : MonoBehaviour
{
    public static RewardSystem Instance { get; private set; }

    [System.Serializable]
    public class LevelReward
    {
        public int level;
        public ItemData rewardItem;
        public int quantity = 1;
    }

    [SerializeField] private List<LevelReward> levelRewards = new List<LevelReward>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public ItemData GetRewardForLevel(int level, out int quantity)
    {
        foreach (var reward in levelRewards)
        {
            if (reward.level == level)
            {
                quantity = reward.quantity;
                return reward.rewardItem;
            }
        }
        quantity = 0;
        return null;
    }
}