using MeshVFX;
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
        
        public ProjectileData explosionProjectile;
        private Projectile _explosionProjectile;

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
                // _vfx = Instantiate(iceExplosionVFXPrefab, _tipIntersectPoint, Quaternion.identity,
                //     null);
                // _vfx.transform.up = _groundNormal;
                // _vfx.Play();
                
                // spawn projectile
                _explosionProjectile = SpawnProjectile(_tipIntersectPoint, _groundNormal);
                Destroy(_explosionProjectile, 0.8f);
            }
        }
        
        private Projectile SpawnProjectile(Vector3 position, Vector3 direction)
        {
            var projectileData = explosionProjectile;
            var projectile = Instantiate(projectileData.projectilePrefab, position,
                Quaternion.identity);
            var projectileComponent = projectile.AddComponent<Projectile>();

            if (projectileComponent != null)
            {
                projectileComponent.Initialize(direction, projectileData, _self.Owner);
            }

            return projectileComponent;
            
        }

        private void OnDestroy()
        {
            // _dissolveEffectController?.PlayEffect(
            //     DissolveEffectController.EffectMode.Dematerialize);
        }
    }
}