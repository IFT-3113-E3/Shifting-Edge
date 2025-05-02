using Enemy.IceBoss.States.Combat;
using Enemy.IceBoss.States.Intro;
using Projectiles;
using Status;
using UnityEngine;
using UnityHFSM;

namespace Enemy.IceBoss
{
    [RequireComponent(typeof(BossAnimator), typeof(BossMovementController),
        typeof(EntityStatus))]
    public class BossController : MonoBehaviour
    {
        [Header("Context initialization")] 
        public GameObject player;
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
            if (GameManager.Instance.GameSession.GameProgression.HasDefeated("IceBoss"))
            {
                gameObject.SetActive(false);
                return;
            }
            
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
            _animator.OnGroundAttackEvent += OnGroundAttack;
            _animator.OnPunchEvent += OnPunch;

            // Init stuff
            InitializeBoss();
        }

        void InitializeBoss()
        {
            player = GameManager.Instance.PlayerManager.Player.GameObject;
            
            Debug.Log("here this" + GameManager.Instance.PlayerManager.Player.gameObject.name);
            _context = new BossContext
            {
                self = gameObject,
                player = player,
                animator = _animator,
                movementController = _movementController,
                entityStatus = _entityStatus,
                orbitCamera = orbitCamera,
                cameraEffects = cameraEffects,
                // speechBubbleSpawner = FindFirstObjectByType<SpeechBubbleSpawner>(),
                spawnPoint = spawnPoint,
            };

            _movementController.ResetAtPointAndOrientation(
                spawnPoint.position, spawnPoint.rotation);
            
            _context.entityStatus.Revive();

            _context.timeSinceLastThrow = _context.throwCooldown;
            _context.timeSinceLastMeleeAttack = _context.meleeAttackCooldown;
            _context.timeSinceLastGroundAttack = _context.groundAttackCooldown;
            _context.waitTimer = _context.attackWaitCooldown / 3f;
            
            _context.shouldActivate = true;
            _context.hasSpawned = true;

            _animator.ResetAnimator();
            
            BuildStateMachine();
            _rootSm.Init();
        }

        void OnThrowSpike(Transform hand)
        {
            if (hand != null)
            {
                var firePos = hand.position;
                var direction = ((_context.player.transform.position + Vector3.up * 0.5f) - firePos).normalized;
                direction.Normalize();
                _ = SpawnProjectile("icespike", firePos, direction);
            }
            else
            {
                Debug.LogError("Hand transform is null.");
            }
        }

        void OnPunch(Transform hand)
        {
            if (hand != null)
            {
                var firePos = hand.position;
                _ = SpawnProjectile("golemgroundattack", firePos, hand.right);
            }
            else
            {
                Debug.LogError("Hand transform is null.");
            }
        }

        void OnGroundAttack()
        {
            var direction = _context.movementController.TransientForward;
            var firePos = _context.self.transform.position + direction * 3f;

            var groundAttackShockwave = SpawnProjectile("golemgroundattack", firePos, direction);
            var distance = Vector3.Distance(firePos,
                _context.player.transform.position);
            var intensity = Mathf.Clamp01(1f - distance / 12f) / 2.5f;
            cameraEffects?.Shake(1f, intensity);

            var projectile = SpawnProjectile("groundspikechain", firePos, direction);
            var chainProjectile = projectile.GetComponent<GroundSpikeChainProjectile>();
            chainProjectile.OnSpawnProjectile += p => p.OnCollision += () =>
            {
                var distance = Vector3.Distance(p.transform.position,
                    _context.player.transform.position);
                var intensity = Mathf.Clamp01(1f - distance / 12f) / 2.5f;
                cameraEffects?.Shake(1f, intensity);
            };
            chainProjectile.SetTarget(_context.player.transform);
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
                afterOnLogic: state =>
                {
                    _context.timeSinceLastMeleeAttack += Time.deltaTime;
                    _context.timeSinceLastThrow += Time.deltaTime;
                    _context.timeSinceLastGroundAttack += Time.deltaTime;
                    
                    var healthPercentage = _context.entityStatus.CurrentHealth /
                        _context.entityStatus.maxHealth;

                    if (healthPercentage <= 0.5f)
                    {
                        if (_context.phase == 0)
                        {
                            _rootSm.Trigger("PhaseChange");
                        }
                        _context.phase = 1;
                    }
                    else
                    {
                        _context.phase = 0;
                    }
                },
                needsExitTime: true
            );
            {
                combatFsm.AddState("Wait", new WaitState(_context));
                
                combatFsm.AddState("Enraged", _ =>
                {
                    _context.animator.PlayEnragedAnimAndCallback(
                        () =>
                        {
                            combatFsm.StateCanExit();
                        });
                }, needsExitTime: true);
                
                combatFsm.AddState("Teleport", new State(needsExitTime: true,
                    onEnter: _ =>
                    {
                        // _context.movementController.StopMovement();
                        var behindPlayerPos = _context.player.transform.position +
                                              _context.player.transform.forward * -2f;

                        _context.animator.TeleportWithExplosion(
                            behindPlayerPos,
                            _context.player.transform.rotation,
                            1f,
                            () =>
                            {
                                // SpawnCircleAttack();
                                _context.movementController.SyncTransform();
                                combatFsm.StateCanExit();
                            });
                        _context.hasJustTeleported = true;
                    },
                    onLogic: _ =>
                    {
                        var behindPlayerPos = _context.player.transform.position +
                                              _context.player.transform.forward * -2f;
                        _context.animator.UpdateTarget(
                            behindPlayerPos,
                            _context.player.transform.rotation);
                    }
                ));

                var chargeFsm = new StateMachine(needsExitTime: true);
                {
                    chargeFsm.AddState("MoveIntoPosition", new MoveIntoPositionState(_context, _context.chargeDistance));

                    chargeFsm.AddState("Charge", new ChargeState(_context));
                    chargeFsm.AddExitTransition("Charge");

                    chargeFsm.AddTransition("MoveIntoPosition", "Charge");
                }
                combatFsm.AddState("Charge", chargeFsm);

                var rangedAttackFsm = new StateMachine(needsExitTime: true);
                {
                    rangedAttackFsm.AddState("MoveIntoPosition", new MoveIntoPositionState(_context, _context.rangedAttackDistance));

                    rangedAttackFsm.AddState("RangedAttack", new RangedAttackState(_context));
                    
                    rangedAttackFsm.AddTransition("RangedAttack", "MoveIntoPosition",
                        _ => _context.phase == 1 && _context.numberOfRepeatedRangedAttacks <
                            (_context.phase == 0 ? 0 : 1), _ =>
                        {
                            _context.numberOfRepeatedRangedAttacks++;
                        });

                    rangedAttackFsm.AddTransition("MoveIntoPosition", "RangedAttack");
                    
                    rangedAttackFsm.AddExitTransition("RangedAttack", onTransition: _ =>
                    {
                        _context.numberOfRepeatedRangedAttacks = 0;
                    });

                }
                combatFsm.AddState("RangedAttack", rangedAttackFsm);

                var groundAttackFsm = new StateMachine(needsExitTime: true);
                {
                    // approach the player, then when close enough begin the attack
                    groundAttackFsm.AddState("Approach", new ApproachState(_context));
                    groundAttackFsm.AddState("Attack", new GroundAttackState(_context));
                    groundAttackFsm.AddState("Finish", new State(isGhostState: true));
                    groundAttackFsm.AddExitTransition("Finish");

                    groundAttackFsm.AddTransition("Approach", "Attack",
                        _ => DistanceToPlayer(5f));
                    groundAttackFsm.AddTransition("Attack", "Finish");
                }
                combatFsm.AddState("GroundAttack", groundAttackFsm);

                combatFsm.AddExitTransition("Wait");

                combatFsm.AddTriggerTransitionFromAny("PhaseChange", "Enraged", forceInstantly: true);
                combatFsm.AddTransition("Enraged", "Wait");
                combatFsm.AddTransition("Wait", "Teleport",
                    _ => _context.waitTimer >= 0.5f && !DistanceToPlayer(15f));
                combatFsm.AddTransition("Wait", "Charge",
                    _ =>
                    {
                        var test = _context.timeSinceLastMeleeAttack >=
                                   _context.meleeAttackCooldown;
                        var facingPlayer = IsPointInView(_context.player.transform.position);
                        var tooManyAttacks = _context.attackHistory.IsLast(AttackType.Melee);//_context.attackHistory.Count > 0 && _context.attackHistory.GetFirst() == AttackType.Melee && _context.attackHistory.ConsecutiveCount >= 1;
                        var isFirst = !_context.attackHistory.Contains(AttackType.Melee) || _context.attackHistory.IsFirst(AttackType.Melee);
                        return test && facingPlayer && !tooManyAttacks && isFirst;
                    });

                combatFsm.AddTransition("Wait", "RangedAttack",
                    _ =>
                    {
                        var tooManyAttacks = _context.attackHistory.IsLast(AttackType.Ranged);//_context.attackHistory.Count > 0 && _context.attackHistory.GetFirst() == AttackType.Ranged && _context.attackHistory.ConsecutiveCount >= 1;
                        var isFirst = !_context.attackHistory.Contains(AttackType.Ranged) || _context.attackHistory.IsFirst(AttackType.Ranged);
                        return _context.timeSinceLastThrow >= _context.throwCooldown &&
                               IsPointInView(_context.player.transform.position) && !tooManyAttacks && isFirst;
                    });
                combatFsm.AddTransition("Wait", "GroundAttack",
                    _ =>
                    {
                        var tooManyAttacks = _context.attackHistory.IsLast(AttackType.Ground);//_context.attackHistory.Count > 0 && _context.attackHistory.GetFirst() == AttackType.Ground && _context.attackHistory.ConsecutiveCount >= 1;
                        var isFirst = !_context.attackHistory.Contains(AttackType.Ground) || _context.attackHistory.IsFirst(AttackType.Ground);
                        return _context.timeSinceLastGroundAttack >=
                               _context.groundAttackCooldown &&
                               IsPointInView(_context.player.transform.position) && !tooManyAttacks && isFirst;
                    });
                
                combatFsm.AddTransition("Charge", "Wait");

                combatFsm.AddTransition("Teleport", "Wait");
                combatFsm.AddTransition("RangedAttack", "Wait");
                combatFsm.AddTransition("GroundAttack", "Wait");

                combatFsm.SetStartState("Wait");
            }
            _rootSm.AddState("Combat", combatFsm);

            var defeatedFsm = new StateMachine(needsExitTime: true);
            {
                defeatedFsm.AddState("Despawning", onEnter: state =>
                {
                    GameManager.Instance.GameSession.GameProgression.MarkBossDefeated("IceBoss");
                    _context.animator.Despawn(() =>
                    {
                        _context.orbitCamera.RemoveAdditionalTarget(_context.animator.transform);
                        _context.animator.AbsorbCore(_context.player.transform, () =>
                        {
                            state.fsm.StateCanExit();
                        });
                    });
                }, needsExitTime: true);
                defeatedFsm.SetStartState("Despawning");
            }
            _rootSm.AddState("Defeated", defeatedFsm);

            _rootSm.AddTransition("Intro", "Combat");
            _rootSm.AddTransition("Combat", "Defeated", _ => _context.entityStatus.CurrentHealth <= 0f,
                forceInstantly: true, onTransition: _ =>
                {
                    _context.defeated = true;
                });

            _rootSm.SetStartState("Intro");
        }

        void Update()
        {
            _context.dt = Time.deltaTime;
            _rootSm.OnLogic();
            //
            // if (Input.GetKeyDown(KeyCode.Y))
            // {
            //     SpawnCircleAttack();
            // }
            //
            //
            // // keyboard input to test the ground spike chain spawning
            // if (Input.GetKeyDown(KeyCode.U))
            // {
            //     OnGroundAttack();
            // }
        }

        private void OnDrawGizmos()
        {
            if (_context == null || _context.self == null)
            {
                return;
            }

            // draw boss view range cone with lines
            var maxViewAngle = 45f / 2f;
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

            var forward = _context.movementController.TransientForward;

            var angleToTarget = Vector3.Angle(forward,
                (position - _context.self.transform.position).normalized);
            var distanceToTarget = Vector3.Distance(_context.self.transform.position, position);

            return angleToTarget <= maxViewAngle;
        }

        private bool IsFacingPoint(Vector3 position, float angleTolerance)
        {
            var forward = _context.movementController.TransientForward;
            var angleToTarget = Vector3.Angle(forward,
                (position - _context.self.transform.position).normalized);
            return angleToTarget <= angleTolerance;
        }

        private bool DistanceToPlayer(float distance)
        {
            return Vector3.Distance(_context.self.transform.position,
                _context.player.transform.position) <= distance;
        }

        private Projectiles.Projectile SpawnProjectile(string projectileName, Vector3 position,
            Vector3 direction)
        {
            if (projectileDatabase != null)
            {
                var projectileData = projectileDatabase.Get(projectileName);
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
                    projectileComponent.Initialize(direction, projectileData,
                        _context.entityStatus);
                }

                return projectileComponent;
            }

            Debug.LogError("Projectile data is not assigned.");
            return null;
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
                _ = SpawnProjectile("groundspike", spawnPos, tiltDirection);
            }
        }

        private void ResetBoss()
        {
            
        }
    }
}