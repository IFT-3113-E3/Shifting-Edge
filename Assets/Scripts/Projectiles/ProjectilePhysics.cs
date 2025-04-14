using UnityEngine;

namespace Projectiles
{

    [CreateAssetMenu(fileName = "ProjectilePhysicsConfig", menuName = "Gameplay/Projectile Physics Config", order = 1)]
    public class ProjectilePhysicsConfig : ScriptableObject
    {
        [Header("General Settings")]
        public float mass = 1f;
        public float gravityMultiplier = 1f;
        public float drag = 0.1f;
        public float angularDrag = 0.05f;
        public float maxSpeed = 20f;
        public bool isKinematic = false;
        
        [Header("Collision Settings")]
        public Vector3 colliderSize = new Vector3(0.5f, 0.5f, 0.5f);
        public Vector3 colliderOffset = Vector3.zero;
        public float bounceFactor = 0.5f;
        public bool sticky = false;
        public float stickDamping = 0.5f;
        public float stickDepth = 0.1f;
        public LayerMask collisionLayer;
        public LayerMask collisionWhitelist;
    }
}