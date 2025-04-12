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
        public CameraEffects cameraEffects;
        public OrbitCamera orbitCamera;
        
        public Transform spawnPoint;
        public SpeechBubbleSpawner speechBubbleSpawner;
        
        public float health = 100f;
        public float maxHealth = 100f;
        public int phase = 0;
        public float attackCooldown = 2f;
        public float throwCooldown = 5f;
        public float timeSinceLastAttack = 0f;
        public float timeSinceLastThrow = 0f;
        public float dashSpeed = 200f;
        public float dashDuration = 1.0f;

        public bool shouldActivate = false;
        public bool hasSpawned = false;
        
        public float dt = 0f;
    }
}