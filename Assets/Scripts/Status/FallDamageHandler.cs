using System;
using UnityEngine;

namespace Status
{
    [RequireComponent(typeof(EntityStatus))]
    public class FallDamageHandler : MonoBehaviour
    {
        [Header("Fall Damage Settings")]
        public float minFallDistance = 3f;
        public float fallSpeedMultiplier = 3f;
        public float damageMultiplier = 10f;

        private EntityStatus _entityStatus;
        private EntityMovementController _characterController;
        private bool _isFalling = false;

        private float _fallStartHeight;
        private float _lastYVelocity;

        private void Awake()
        {
            _entityStatus = GetComponent<EntityStatus>();
            _characterController = GetComponent<EntityMovementController>();
        }

        private void OnEnable()
        {
            _characterController.OnGroundHitVelocity += HandleGroundHit;
        }
        
        private void OnDisable()
        {
            _characterController.OnGroundHitVelocity -= HandleGroundHit;
        }
        
        private void HandleGroundHit(Vector3 velocity)
        {
            float currentY = transform.position.y;
            if (_isFalling)
            {
                _isFalling = false;

                float fallDistance = _fallStartHeight - currentY;
                float impactSpeed = -velocity.y * fallSpeedMultiplier;
                Debug.Log($"Falling distance: {fallDistance}, Speed: {impactSpeed}");

                if (fallDistance > minFallDistance)
                {
                    float maxHeight = minFallDistance + 5f;
                    float damageFromHeight = Mathf.InverseLerp(minFallDistance, maxHeight, fallDistance);
                    float speedMultiplier = Mathf.Max(0f, impactSpeed);
                    float damage = damageMultiplier * speedMultiplier * damageFromHeight;
                    Debug.Log($"Fall damage: {damage}");

                    ApplyFallDamage(damage);
                }
            }
        }

        private void Update()
        {
            bool grounded = IsGrounded();
            float currentY = transform.position.y;

            if (!grounded)
            {
                if (!_isFalling)
                {
                    _isFalling = true;
                    _fallStartHeight = currentY;
                }
            }

            
            if (_lastYVelocity >= 0f && _characterController.Velocity.y < 0f)
            {
                // Player is falling down
                _fallStartHeight = currentY;
            }

            _lastYVelocity = _characterController.Velocity.y;
        }

        private void ApplyFallDamage(float damage)
        {
            if (damage <= float.Epsilon) return;

            DamageRequest fallDamage = new DamageRequest(
                damage,
                null,
                transform.position
            );

            _entityStatus.ApplyDamage(fallDamage);
        }

        private bool IsGrounded()
        {
            return _characterController.IsGrounded;
        }
    }
}
