using Enemy.IceBoss.States.Combat;
using Enemy.IceBoss.States.Intro;
using MeshVFX;
using Projectiles;
using Status;
using UI;
using UnityEngine;
using UnityHFSM;

namespace Enemy.IceBoss
{
    [RequireComponent(typeof(BossAnimator), typeof(BossMovementController),
        typeof(EntityStatus))]
    public class BossController : MonoBehaviour
    {
        [Header("Context initialization")] public GameObject player;
        public Transform spawnPoint;
        
        public CameraEffects cameraEffects;
        public OrbitCamera orbitCamera;

        public ProjectileDatabase projectileDatabase;

        [SerializeField] private BossContext _context;
        private StateMachine _rootSm;

        private BossAnimator _animator;
        private BossMovementController _movementController;
        private EntityStatus _entityStatus;
        

        // Used mainly for debugging
        public StateMachine RootSm => _rootSm;
        public BossContext Context => _context;

        void Start()
        {
            if (!cameraEffects)
            {
                cameraEffects = FindFirstObjectByType<CameraEffects>();
                if (cameraEffects == null)
                {
                    Debug.LogWarning("[BossController] CameraEffects component not found!");
                }
            }
            if (!orbitCamera)
            {
                orbitCamera = FindFirstObjectByType<OrbitCamera>();
                if (orbitCamera == null)
                {
                    Debug.LogWarning("[BossController] OrbitCamera component not found!");
                }
            }
            
            _animator = GetComponent<BossAnimator>();
            if (_animator == null)
            {
                Debug.LogError("[BossController] Animator component not found!");
            }

            _movementController = GetComponent<BossMovementController>();
            if (_movementController == null)
            {
                Debug.LogError("[BossController] MovementController component not found!");
            }

            _entityStatus = GetComponent<EntityStatus>();
            if (_entityStatus == null)
            {
                Debug.LogError("[BossController] StatusEffectManager component not found!");
            }
            
            // Pre-init stuff
            _animator.OnThrowEvent += OnThrowSpike;

            // Init stuff
            _context = new BossContext
            {
                self = gameObject,
                player = player,
                animator = _animator,
                movementController = _movementController,
                entityStatus = _entityStatus,
                orbitCamera = orbitCamera,
                cameraEffects = cameraEffects,
                speechBubbleSpawner = FindFirstObjectByType<SpeechBubbleSpawner>(),
                spawnPoint = spawnPoint,
            };


            InitializeBoss();

            BuildStateMachine();
            _rootSm.Init();
        }

        void InitializeBoss()
        {
            _context.shouldActivate = true;
            _context.hasSpawned = true;

            var fractureEffect = GetComponent<FractureEffect>();
            if (fractureEffect != null)
            {
                fractureEffect.SwapVisibility(false);
            }
        }

        void OnThrowSpike(Transform hand)
        {
            if (hand != null)
            {
                var firePos = hand.position;
                var direction = (_context.player.transform.position - firePos).normalized;
                direction.Normalize();
                SpawnProjectile("icespike", firePos, direction);
            }
            else
            {
                Debug.LogError("Hand transform is null.");
            }
        }

        void BuildStateMachine()
        {
            _rootSm = new StateMachine();

            var introFsm = new StateMachine(needsExitTime: true);
            {
                introFsm.AddState("Dormant", new DormantState(_context));
                introFsm.AddState("Spawning", new SpawningState(_context));
                introFsm.AddState("Engaged", new EngagedState(this, _context));

                introFsm.AddExitTransition("Engaged");

                introFsm.AddTransition("Dormant", "Spawning", _ => _context.shouldActivate);
                introFsm.AddTransition("Spawning", "Engaged", _ => _context.hasSpawned);

                introFsm.SetStartState("Dormant");
            }
            _rootSm.AddState("Intro", introFsm);

            var combatFsm = new HybridStateMachine(
                afterOnLogic: _ =>
                {
                    _context.timeSinceLastAttack += Time.deltaTime;
                    _context.timeSinceLastThrow += Time.deltaTime;
                },
                needsExitTime: true
            );
            {
                combatFsm.AddState("Wait", new WaitState(_context));
                combatFsm.AddState("Teleport", new State(needsExitTime: true,
                    onEnter: _ =>
                    {
                        _context.movementController.StopMovement();
                        var behindPlayerPos = _context.player.transform.position +
                                              _context.player.transform.forward * -2f;

                        _context.animator.TeleportWithExplosion(
                            behindPlayerPos,
                            _context.player.transform.rotation,
                            () =>
                            {
                                // SpawnCircleAttack();
                                combatFsm.StateCanExit();
                            });
                    },
                    onLogic: _ =>
                    {
                        var behindPlayerPos = _context.player.transform.position +
                                              _context.player.transform.forward * -2f;
                        _context.animator.UpdateTarget(
                            behindPlayerPos,
                            _context.player.transform.rotation);
                    },
                    onExit: _ =>
                    {
                        _context.movementController.StopMovement();
                    }
                ));
                combatFsm.AddState("Charge", new ChargeState(_context));
                combatFsm.AddState("RangedAttack", new RangedAttackState(_context));

                combatFsm.AddExitTransition("Wait");
                combatFsm.AddTransition("Wait", "Charge",
                    _ =>
                    {
                        var test = _context.timeSinceLastAttack >= _context.attackCooldown;
                        var facingPlayer = IsPointInView(_context.player.transform.position);
                        return test && facingPlayer;
                    });

                combatFsm.AddTransition("Wait", "RangedAttack",
                    _ =>
                    {
                        return _context.timeSinceLastThrow >= _context.throwCooldown 
                               && !DistanceToPlayer(15f);
                    });
                combatFsm.AddTransition("Charge", "Wait");
                combatFsm.AddTransition("Wait", "Teleport",
                    _ => _context.timeSinceLastAttack >= 0.8f && !DistanceToPlayer(20f));
                
                combatFsm.AddTransition("Teleport", "Wait");

                combatFsm.AddTransition("RangedAttack", "Wait");

                combatFsm.SetStartState("Wait");
            }
            _rootSm.AddState("Combat", combatFsm);

            var defeatedFsm = new StateMachine(needsExitTime: true);
            {
                // defeatedFsm.AddState("Despawning", new DespawningState(_context));
            }
            _rootSm.AddState("Defeated", defeatedFsm);

            _rootSm.AddTransition("Intro", "Combat");
            _rootSm.AddTransition("Combat", "Defeated", _ => _context.health <= 0f,
                forceInstantly: true);

            _rootSm.SetStartState("Intro");
        }

        void Update()
        {
            _context.dt = Time.deltaTime;
            _rootSm.OnLogic();
            
            if (Input.GetKeyDown(KeyCode.Y))
            {
                SpawnCircleAttack();
            }
        }
        

        private void OnDrawGizmos()
        {
            if (_context == null || _context.self == null)
            {
                return;
            }
            // draw boss view range cone with lines
            var maxViewAngle = 45f/2f;
            var viewDistance = 15f;
            var viewAngle = maxViewAngle * 2f;
            var angleStep = viewAngle / 10f;
            var startAngle = -maxViewAngle;
            var endAngle = maxViewAngle;
            var startPos = _context.self.transform.position;
            var forward = _context.movementController.TransientForward;
            var right = Quaternion.Euler(0f, startAngle, 0f) * forward;
            
            var endPos = Quaternion.Euler(0f, endAngle, 0f) * forward;
            Gizmos.color = Color.red;
            {
                Gizmos.DrawLine(startPos, startPos + right * viewDistance);
                Gizmos.DrawLine(startPos, startPos + endPos * viewDistance);
                for (float angle = startAngle; angle <= endAngle; angle += angleStep)
                {
                    var direction = Quaternion.Euler(0f, angle, 0f) * forward;
                    Gizmos.DrawLine(startPos, startPos + direction * viewDistance);
                }
                
            }
        }

        private bool IsPointInView(Vector3 position)
        {
            var maxViewAngle = 45f / 2f;
            var viewDistance = 15f;
            var viewAngle = maxViewAngle * 2f;

            var forward = _context.movementController.TransientForward;
            var right = Quaternion.Euler(0f, -maxViewAngle, 0f) * forward;
            var left = Quaternion.Euler(0f, maxViewAngle, 0f) * forward;

            var angleToTarget = Vector3.Angle(forward, (position - _context.self.transform.position).normalized);
            var distanceToTarget = Vector3.Distance(_context.self.transform.position, position);

            return angleToTarget <= maxViewAngle && distanceToTarget <= viewDistance;
        }
        
        private bool DistanceToPlayer(float distance)
        {
            return Vector3.Distance(_context.self.transform.position,
                _context.player.transform.position) <= distance;
        }

        private void SpawnProjectile(string name, Vector3 position, Vector3 direction)
        {
            if (projectileDatabase != null)
            {
                var projectileData = projectileDatabase.Get(name);
                var projectile = Instantiate(projectileData.projectilePrefab, position,
                    Quaternion.identity);
                var projectileComponent = projectile.AddComponent<Projectiles.Projectile>();
                // screen shake with higher intensity when closer to the player
                projectileComponent.OnCollision += () =>
                {
                    var distance = Vector3.Distance(projectile.transform.position,
                        _context.player.transform.position);
                    var intensity = Mathf.Clamp01(1f - distance / 12f) / 2.5f;
                    cameraEffects?.Shake(1f, intensity);
                };
                if (projectileComponent != null)
                {
                    projectileComponent.Initialize(direction, projectileData);
                }
            }
            else
            {
                Debug.LogError("Projectile data is not assigned.");
            }
        }

        // Spawns a circle arrangement of projectiles from under the ground around the boss
        private void SpawnCircleAttack()
        {
            Vector3 position = _context.self.transform.position;
            var projectileCount = 8;
            var radius = 4f;
            var angleStep = 360f / projectileCount;

            for (int i = 0; i < projectileCount; i++)
            {
                float angle = i * angleStep;
                Vector3 spawnPos = new Vector3(
                    position.x + radius * Mathf.Cos(angle * Mathf.Deg2Rad),
                    position.y,
                    position.z + radius * Mathf.Sin(angle * Mathf.Deg2Rad)
                );

                // outer tilt direction from the boss
                var tiltAngleFromVertical = 10f;
                var tiltDirection = Quaternion.Euler(tiltAngleFromVertical, angle, 0f) * Vector3.up;
                SpawnProjectile("groundspike", spawnPos, tiltDirection);
            }
        }
    }
}