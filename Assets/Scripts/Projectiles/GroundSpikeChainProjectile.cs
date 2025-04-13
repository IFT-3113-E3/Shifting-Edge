using System;
using UnityEngine;

namespace Projectiles
{
    public class GroundSpikeChainProjectile : MonoBehaviour
    {
        public float steeringSpeed = 60f;
        public float spawnInterval = 0.25f;
        public ProjectileData spikeProjectileData;

        private Transform _target;
        private Projectile _self;

        private float _timeSinceLastSpawn;

        
        public event Action<Projectile> OnSpawnProjectile;
        
        public void SetTarget(Transform target)
        {
            _target = target;
        }

        private void Start()
        {
            if (_target == null)
            {
                Debug.LogError("Target not set for GroundSpikeChainProjectile.");
                Destroy(gameObject);
            }

            _self = GetComponent<Projectile>();
            if (_self == null)
            {
                Debug.LogError("Projectile component not found on GroundSpikeChainProjectile.");
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            if (_target != null)
            {
                Vector3 direction = (_target.position - transform.position).normalized;
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation,
                    steeringSpeed * Time.deltaTime);

                // Move the projectile forward
                transform.position += transform.forward * _self.projectileData.speed * Time.deltaTime;
            }
            else
            {
                Debug.LogError("Target lost for GroundSpikeChainProjectile.");
                Destroy(gameObject);
            }

            // Spawn spikes at intervals
            _timeSinceLastSpawn += Time.deltaTime;
            if (_timeSinceLastSpawn >= spawnInterval)
            {
                SpawnSpike();
                _timeSinceLastSpawn = 0f;
            }
        }

        private void SpawnSpike()
        {
            Vector3 groundPos = FindGroundPosition(transform.position);
            Vector3 spawnPosition = groundPos + Vector3.up * 0.5f;
            Vector3 direction = Vector3.up;
            var projectile = SpawnProjectile(spawnPosition, direction);
            OnSpawnProjectile?.Invoke(projectile);
        }
        
        private Vector3 FindGroundPosition(Vector3 position)
        {
            RaycastHit hit;
            if (Physics.Raycast(position, Vector3.down, out hit, 20f))
            {
                return hit.point;
            }

            return position; // Return original position if no ground is found
        }

        private Projectile SpawnProjectile(Vector3 position, Vector3 direction)
        {
            var projectile = Instantiate(spikeProjectileData.projectilePrefab, position,
                Quaternion.identity);
            var projectileComponent = projectile.AddComponent<Projectile>();

            if (projectileComponent != null)
            {
                projectileComponent.Initialize(direction, spikeProjectileData, _self.Owner);
            }

            return projectileComponent;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * 5f);
            Gizmos.DrawWireSphere(transform.position, 0.5f);
            Gizmos.DrawWireSphere(transform.position + transform.forward * 5f, 0.5f);
        }
    }
}