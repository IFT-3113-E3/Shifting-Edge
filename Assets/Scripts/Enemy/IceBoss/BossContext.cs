using System;
using System.Collections.Generic;
using Status;
using UI;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;

namespace Enemy.IceBoss
{
    public enum AttackType
    {
        Ranged,
        Melee,
        Ground
    }
    
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
        public BossFightState bossFightState;
        
        public Transform spawnPoint;
        
        public int phase = 0;
        public float attackWaitCooldown = 3f;
        public float meleeAttackCooldown = 3f;
        public float throwCooldown = 5f;
        public float groundAttackCooldown = 8f;
        public float waitTimer = 0f;
        public float timeSinceLastMeleeAttack = 0f;
        public float timeSinceLastThrow = 0f;
        public float timeSinceLastGroundAttack = 0f;
        public float dashSpeed = 320f;
        public float chargeDistance = 8f;
        public float rangedAttackDistance = 15f;
        public float lookAtSpeed = 100f;
        public int numberOfRepeatedRangedAttacks = 0;
        public bool hasJustTeleported = false;
        
        public RecentSet<AttackType> attackHistory = new() { AttackType.Ground, AttackType.Ranged, AttackType.Melee };

        public bool shouldActivate = false;
        public bool hasSpawned = false;
        public bool defeated = false;
        
        public float dt = 0f;
    }
}