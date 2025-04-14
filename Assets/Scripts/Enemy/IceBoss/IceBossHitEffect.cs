using Status;
using UnityEngine;

namespace Enemy.IceBoss
{
    public class IceBossHitEffect : MonoBehaviour, IHitReaction
    {
        private EntityMovementController _mc;
        private BossAnimator _animator;
        
        private void Awake()
        {
            _mc = GetComponent<EntityMovementController>();
            _animator = GetComponent<BossAnimator>();
        }

        public void ReactToHit(DamageRequest damageRequest)
        {
            _animator.HurtEffect();
            
            if (_mc != null)
            {
                // _mc.CancelVelocity();
                // var knockbackDirection = damageRequest.knockbackDirection;
                // knockbackDirection.y = 0;
                // knockbackDirection.Normalize();
                // _mc.AddKnockback(knockbackDirection, damageRequest.knockbackForce,
                //     5.0f);
            }
        }
    }
}