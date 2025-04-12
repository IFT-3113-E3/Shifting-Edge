using System;
using UnityEngine;

namespace Projectiles
{
    public class Projectile : MonoBehaviour
    {
        public ProjectileData projectileData;

        private Rigidbody _rb;
        private Collider _collisionCollider;
        private Collider _hitboxCollider;
        private int _bounceCount;
        private bool _hasCollided;
        private bool _isHitboxEnabled = true;
        private float _lifeTime;

        private bool _isPenetrating = false;
        private float _penetratedDistance = 0f;
        private Vector3 _penetrationPoint;
        private Vector3 _penetrationDirection;

        private Vector3 _lastPosition;
        private Quaternion _lastRotation;
        private Vector3 _lastVelocity;
        private Vector3 _lastAngularVelocity;
        private Vector3 _tipIntersectionPoint;

        private AudioSource _audioSource;

        public event Action OnCollision;

        public event Action<Collider> OnHitEntity;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _collisionCollider = GetComponent<Collider>();

            Collider[] colliders = GetComponentsInChildren<Collider>();
            foreach (Collider col in colliders)
            {
                if (col != _collisionCollider && col.isTrigger)
                {
                    _hitboxCollider = col;
                    break;
                }
            }
            
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
                _audioSource.playOnAwake = false;
                _audioSource.spatialBlend = 1f; 
                _audioSource.dopplerLevel = 0f;
                _audioSource.rolloffMode = AudioRolloffMode.Linear;
                _audioSource.minDistance = 1f;
                _audioSource.maxDistance = 100f;
                _audioSource.volume = 1f;
                _audioSource.pitch = 1f;
            }

            if (_rb == null)
            {
                Debug.LogError("Projectile prefab needs a Rigidbody component.");
                enabled = false;
            }

            if (_collisionCollider == null)
            {
                Debug.LogError("Projectile prefab needs a Collider component for collision.");
                enabled = false;
            }

            if (_hitboxCollider == null)
            {
                Debug.LogWarning(
                    "Projectile prefab should have a child Collider set to 'Is Trigger' for the hitbox.");
            }
        }

        public void Initialize(Vector3 direction, ProjectileData data)
        {
            projectileData = data;

            _isHitboxEnabled = true;
            _bounceCount = 0;
            _hasCollided = false;
            _lifeTime = 0f;
            _rb.isKinematic = false;
            if (projectileData.physicsConfig.isKinematic)
            {
                _rb.constraints = RigidbodyConstraints.FreezeAll;
            }
            _rb.useGravity = projectileData.physicsConfig.gravityMultiplier > 0;
            transform.SetParent(null);

            transform.rotation = Quaternion.LookRotation(direction);

            if (projectileData == null)
            {
                Debug.LogError("Projectile Data not assigned to the projectile.");
                Destroy(gameObject);
                return;
            }

            _rb.mass = projectileData.physicsConfig.mass;
            _rb.linearDamping = projectileData.physicsConfig.drag;
            _rb.angularDamping = projectileData.physicsConfig.angularDrag;
            if (!projectileData.physicsConfig.isKinematic)
            {
                _rb.linearVelocity = direction.normalized * projectileData.speed;
            }

            _lastPosition = transform.position;
            _lastRotation = transform.rotation;
            _lastVelocity = direction.normalized * projectileData.speed;
            _lastAngularVelocity = Vector3.zero;

            gameObject.layer = LayerMask.NameToLayer(LayerMask.LayerToName(
                Mathf.RoundToInt(Mathf.Log(projectileData.physicsConfig.collisionLayer.value, 2))));

            if (_hitboxCollider)
            {
                _hitboxCollider.gameObject.layer = LayerMask.NameToLayer(
                    LayerMask.LayerToName(
                        Mathf.RoundToInt(Mathf.Log(projectileData.hitboxLayerMask.value, 2))));
            }

            if (projectileData.script)
            {
                MonoBehaviour behavior =
                    Instantiate(projectileData.script, transform);
            }

            _hasCollided = false;
            _bounceCount = 0;
            
            PlaySpawnSound();
        }

        private void Update()
        {
            _lifeTime += Time.deltaTime;
            if (_lifeTime >= projectileData.lifetime)
            {
                DestroySelf();
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (_hasCollided) return; // Only process the first collision

            OnCollision?.Invoke();
            
            PlayHitSound();

            if ((projectileData.physicsConfig.collisionWhitelist &
                 (1 << collision.gameObject.layer)) != 0)
            {
                _hasCollided = true;

                if (projectileData.disableHitboxOnCollision && _hitboxCollider != null)
                {
                    _hitboxCollider.enabled = false;
                }

                if (projectileData.physicsConfig.isKinematic) return;
                if (projectileData.physicsConfig.sticky)
                {
                    _isPenetrating = true;
                    _penetratedDistance = 0f;
                    _penetrationDirection = _lastVelocity.normalized;
                    _penetrationPoint = _rb.position;

                    _collisionCollider.enabled = false;
                    Physics.IgnoreCollision(_collisionCollider, collision.collider, true);
                    // reset the velocity to before the collision
                    transform.position = _lastPosition;
                    transform.rotation = _lastRotation;
                    _rb.linearVelocity = _lastVelocity;
                    _rb.angularVelocity = _lastAngularVelocity;

                    _rb.useGravity = false;
                    _isHitboxEnabled = false;

                    Debug.Log(
                        $"Projectile has collided with {collision.gameObject.name} and is now penetrating.");

                    // freeze the projectile's rotation
                    _rb.constraints = RigidbodyConstraints.FreezeRotation;
                }
                else if (projectileData.canBounce && _bounceCount < projectileData.maxBounces)
                {
                    _bounceCount++;
                    Vector3 reflectedVelocity =
                        Vector3.Reflect(_rb.linearVelocity, collision.contacts[0].normal);
                    _rb.linearVelocity = reflectedVelocity;
                    _hasCollided = false;
                }
                else
                {
                    DestroySelf();
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_isHitboxEnabled) return; // Don't trigger hitbox after main collision

            if (_hitboxCollider != null && other == _hitboxCollider) return; // Ignore self-trigger

            if ((projectileData.hitboxLayerMask & (1 << other.gameObject.layer)) != 0)
            {
                OnHitEntity?.Invoke(other);
                Debug.Log($"{name} projectile has hit entity: {other.name}");
            }
        }

        private void FixedUpdate()
        {
            if (_isPenetrating)
            {
                float speed = _rb.linearVelocity.magnitude;
                float dt = Time.fixedDeltaTime;

                // Calculate how far we penetrated this frame
                float moveDistance = speed * dt;
                _penetratedDistance += moveDistance;

                // Dampen the velocity (exponential damping)
                float damping = projectileData.physicsConfig.stickDamping;
                // _rb.linearVelocity = Vector3.Lerp(_rb.linearVelocity, Vector3.zero, damping * dt);
                Debug.Log($"Projectile is penetrating: {_rb.linearVelocity.magnitude}");

                // Stop when we reach the penetration depth or velocity is very small
                if (_penetratedDistance >= projectileData.physicsConfig.stickDepth ||
                    _rb.linearVelocity.magnitude < 0.01f)
                {
                    _rb.linearVelocity = Vector3.zero;
                    _rb.isKinematic = true;
                    _isPenetrating = false;

                    // ensure the projectile is at the penetration point + the penetration direction * penetration depth
                    transform.position = _penetrationPoint +
                                         _penetrationDirection *
                                         projectileData.physicsConfig.stickDepth;
                    transform.rotation = Quaternion.LookRotation(_penetrationDirection);
                }

                return; // Skip gravity while penetrating
            }

            // simulate multiplied gravity by applying constant force for gravity factor
            var gravityFactor = projectileData.physicsConfig.gravityMultiplier;
            if (gravityFactor > 0)
            {
                Vector3 currentGravity = Physics.gravity;
                Vector3 newGravity = Physics.gravity * gravityFactor;
                _rb.AddForce(newGravity - currentGravity, ForceMode.Acceleration);
            }

            _lastPosition = _rb.position;
            _lastRotation = _rb.rotation;
            _lastVelocity = _rb.linearVelocity;
            _lastAngularVelocity = _rb.angularVelocity;
        }

        private void PlaySpawnSound()
        {
            if (projectileData.spawnSound != null && projectileData.spawnSound.Length > 0 && _audioSource != null)
            {
                var spawnSoundClips = projectileData.spawnSound;
                _audioSource.PlayOneShot(spawnSoundClips[UnityEngine.Random.Range(0, spawnSoundClips.Length)]);
            }
        }
        
        private void PlayHitSound()
        {
            if (projectileData.hitSound != null && projectileData.hitSound.Length > 0 && _audioSource != null)
            {
                var hitSoundClips = projectileData.hitSound;
                _audioSource.PlayOneShot(hitSoundClips[UnityEngine.Random.Range(0, hitSoundClips.Length)]);
            }
        }
        
        private void PlayDestroySound()
        {
            if (projectileData.destroySound != null && projectileData.destroySound.Length > 0 && _audioSource != null)
            {
                var destroySoundClips = projectileData.destroySound;
                _audioSource.PlayOneShot(destroySoundClips[UnityEngine.Random.Range(0, destroySoundClips.Length)]);
            }
        }
        
        private void DestroySelf()
        {
            if (_audioSource != null)
            {
                PlayDestroySound();
            }

            Destroy(gameObject);
        }
    }
}