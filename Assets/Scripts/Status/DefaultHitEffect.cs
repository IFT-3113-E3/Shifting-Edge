using MeshVFX;
using UnityEngine;

namespace Status
{
    public class DefaultHitEffect : MonoBehaviour, IHitReaction
    {
        public Color flashColor = Color.red;
        public float flashDuration = 0.1f;
        public ParticleSystem hitParticles;
        private MeshFlashEffect _meshFlashEffect;
        private EntityMovementController _mc;

        private void Awake()
        {
            hitParticles = GetComponentInChildren<ParticleSystem>();
            if (hitParticles != null)
            {
                hitParticles.Stop();
            }

            _meshFlashEffect = GetComponent<MeshFlashEffect>();
            _mc = GetComponent<EntityMovementController>();
        }

        public void ReactToHit(DamageRequest damageRequest)
        {
            var hitPoint = damageRequest.hitPoint;

            // if (_flashRoutine != null)
            //     StopCoroutine(_flashRoutine);
            // _flashRoutine = StartCoroutine(FlashRoutine());
            if (_meshFlashEffect != null)
            {
                _meshFlashEffect.SetOptions(new FlashOptions
                {
                    UseFade = true,
                    BaseAlpha = 0.6f,
                    Interval = 0.5f,
                    FadeTime = 0.25f,
                    ColorOverride = flashColor
                });
                _meshFlashEffect.FlashOnce(flashDuration);
            }

            if (hitParticles != null)
            {
                hitParticles.transform.position = hitPoint;
                hitParticles.Clear();
                hitParticles.Simulate(hitParticles.main.duration);
                hitParticles.Play();
            }

            if (_mc != null)
            {
                _mc.CancelVelocity();
                var knockbackDirection = damageRequest.knockbackDirection;
                knockbackDirection.y = 0;
                knockbackDirection.Normalize();
                _mc.AddKnockback(knockbackDirection, damageRequest.knockbackForce,
                    5.0f);
            }
        }

        // private IEnumerator FlashRoutine()
        // {
        //     if (_meshFlashEffect != null)
        //     {
        //         _meshFlashEffect.SetOptions(new FlashOptions
        //         {
        //             UseFade = true,
        //             BaseAlpha = 0.6f,
        //             Interval = 0.5f,
        //             FadeTime = 0.25f,
        //             ColorOverride = flashColor
        //         });
        //         _meshFlashEffect.FlashOnce(flashDuration);
        //     }
        //
        // }
    }
}
