using UnityEngine;

namespace Projectiles
{

    [CreateAssetMenu(fileName = "ProjectileData", menuName = "Gameplay/Projectile Data", order = 0)]
    public class ProjectileData : ScriptableObject
    {
        [Header("Prefab and Basics")]
        public string projectileName;
        public GameObject projectilePrefab;
        public float speed = 10f;
        public float lifetime = 5f;
        
        [Header("Damage and Hitbox")]
        public float damage = 10f;
        public float knockback = 0f;
        public bool piercing = false;
        
        public LayerMask hitboxLayerMask;

        [Header("Collision Behavior")]
        public bool canBounce = false;
        public int maxBounces = 0;
        public bool disableHitboxOnCollision = false;
        
        [Header("Physics")]
        public ProjectilePhysicsConfig physicsConfig;

        [Header("Optional Scripted Behavior")]
        public MonoBehaviour script;
    }
}