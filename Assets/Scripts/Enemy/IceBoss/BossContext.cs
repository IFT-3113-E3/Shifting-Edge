using System;
using Status;
using UI;
using UnityEngine;

namespace Enemy.IceBoss
{
    [Serializable]
    public class BossContext
    {
        public GameObject self;
        public GameObject player;
        public BossAnimator animator;
        public BossMovementController movementController;
        public EntityStatus entityStatus;
        
        public Transform spawnPoint;
        public SpeechBubbleSpawner speechBubbleSpawner;
        
        public float health = 100f;
        public float maxHealth = 100f;
        public int phase = 0;
        public float attackCooldown = 2f;
        public float throwCooldown = 5f;
        public float timeSinceLastAttack = 0f;
        public float timeSinceLastThrow = 0f;
        public float dashSpeed = 10f;
        public float dashDuration = 0.5f;

        public bool shouldActivate = false;
        public bool hasSpawned = false;
    }
}