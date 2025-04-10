using UnityEngine;
using UnityEngine.VFX;

namespace Projectiles
{
    public class IceSpikeProjectile : MonoBehaviour
    {
        public VisualEffect iceExplosionVFXPrefab;

        private Projectile _self;
        private DissolveEffectController _dissolveEffectController;
        private VisualEffect _vfx;

        private Vector3 _tipIntersectPoint;
        private Vector3 _groundNormal;

        private void Start()
        {
            _self = GetComponent<Projectile>();
            if (_self == null)
            {
                Debug.LogError("[IceSpikeProjectile] Projectile component not found!");
            }

            _dissolveEffectController = GetComponent<DissolveEffectController>();
            if (_dissolveEffectController == null)
            {
                Debug.LogError(
                    "[IceSpikeProjectile] DissolveEffectController component not found!");
            }

            // _dissolveEffectController.PlayEffect(DissolveEffectController.EffectMode.Materialize);

            _self.OnCollision += OnCollision;
        }

        private void FixedUpdate()
        {
            if (Physics.Raycast(transform.position, transform.forward, out var hit, 20f))
            {
                _tipIntersectPoint = hit.point;
                _groundNormal = hit.normal;
            }
        }

        private void OnCollision()
        {
            // Spawn the ice explosion VFX at the tip intersect point
            if (iceExplosionVFXPrefab != null)
            {
                _vfx = Instantiate(iceExplosionVFXPrefab, _tipIntersectPoint, Quaternion.identity,
                    null);
                _vfx.transform.up = _groundNormal;
                _vfx.Play();
                Destroy(_vfx.gameObject, 2f);
            }
        }

        private void OnDestroy()
        {
            // _dissolveEffectController?.PlayEffect(
            //     DissolveEffectController.EffectMode.Dematerialize);
        }
    }
}