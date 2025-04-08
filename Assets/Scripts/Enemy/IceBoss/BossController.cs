using System;
using Enemy.IceBoss.States;
using Enemy.IceBoss.States.Combat;
using Enemy.IceBoss.States.Intro;
using Status;
using UI;
using UnityEngine;
using UnityHFSM;

namespace Enemy.IceBoss
{
    [RequireComponent(typeof(BossAnimator), typeof(BossMovementController),
        typeof(StatusEffectManager))]
    public class BossController : MonoBehaviour
    {
        [Header("Context initialization")] public GameObject player;
        public Transform spawnPoint;

        [SerializeField] private BossContext _context;
        private StateMachine _rootSm;

        private BossAnimator _animator;
        private BossMovementController _movementController;
        private StatusEffectManager _statusEffectManager;

        // Used mainly for debugging
        public StateMachine RootSm => _rootSm;
        public BossContext Context => _context;

        void Start()
        {
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

            _statusEffectManager = GetComponent<StatusEffectManager>();
            if (_statusEffectManager == null)
            {
                Debug.LogError("[BossController] StatusEffectManager component not found!");
            }

            _context = new BossContext
            {
                self = gameObject,
                player = player,
                animator = _animator,
                movementController = _movementController,
                statusEffectManager = _statusEffectManager,
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
                afterOnLogic: _ => { _context.timeSinceLastAttack += Time.deltaTime; },
                needsExitTime: true
            );
            {
                combatFsm.AddState("Wait", new WaitState(_context));
                combatFsm.AddState("Teleport", new State(needsExitTime: true,
                    onEnter: _ =>
                    {
                        _context.movementController.StopMovement();
                        _context.movementController.enableIdleFloat = false;
                        var behindPlayerPos = _context.player.transform.position +
                                              _context.player.transform.forward * -2f;
                        _context.animator.TeleportWithExplosion(
                            behindPlayerPos,
                            _context.player.transform.rotation,
                            () => { combatFsm.StateCanExit(); });
                    },
                    onExit: _ =>
                    {
                        _context.movementController.StopMovement();
                        _context.movementController.enableIdleFloat = true;
                    }
                ));
                combatFsm.AddState("Charge", new ChargeState(_context));

                combatFsm.AddExitTransition("Wait");
                combatFsm.AddTransition("Wait", "Charge",
                    _ =>
                    {
                        var test = _context.timeSinceLastAttack >= _context.attackCooldown;
                        // not facing player
                        var facingPlayer = isPointInView(_context.player.transform.position);
                        return test && facingPlayer;
                    });
                combatFsm.AddTransition("Charge", "Wait");
                combatFsm.AddTransition("Wait", "Teleport",
                    _ => _context.timeSinceLastAttack >= 0.8f && Vector3.Distance(
                        _context.self.transform.position,
                        _context.player.transform.position) > 10f);
                
                combatFsm.AddTransition("Teleport", "Wait");

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
            _rootSm.OnLogic();
        }

        private void OnDrawGizmos()
        {
            if (_context == null || _context.self == null)
            {
                return;
            }
            // draw boss view range cone with lines
            var maxViewAngle = 45f/2f;
            var viewDistance = 10f;
            var viewAngle = maxViewAngle * 2f;
            var angleStep = viewAngle / 10f;
            var startAngle = -maxViewAngle;
            var endAngle = maxViewAngle;
            var startPos = _context.self.transform.position;
            var forward = _context.self.transform.forward;
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

        private bool isPointInView(Vector3 position)
        {
            var maxViewAngle = 45f / 2f;
            var viewDistance = 10f;
            var viewAngle = maxViewAngle * 2f;

            var forward = _context.self.transform.forward;
            var right = Quaternion.Euler(0f, -maxViewAngle, 0f) * forward;
            var left = Quaternion.Euler(0f, maxViewAngle, 0f) * forward;

            var angleToTarget = Vector3.Angle(forward, (position - _context.self.transform.position).normalized);
            var distanceToTarget = Vector3.Distance(_context.self.transform.position, position);

            return angleToTarget <= maxViewAngle && distanceToTarget <= viewDistance;
        }
    }
}