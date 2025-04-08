using Enemy.IceBoss.States;
using Enemy.IceBoss.States.Combat;
using Enemy.IceBoss.States.Intro;
using Status;
using UI;
using UnityEngine;
using UnityHFSM;

namespace Enemy.IceBoss
{
    [RequireComponent(typeof(BossAnimator), typeof(BossMovementController), typeof(StatusEffectManager))]
    public class BossController : MonoBehaviour
    {
        [Header("Context initialization")] 
        public GameObject player;
        public Transform spawnPoint;

        private BossContext _context;
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

            var combatFsm = new HybridStateMachine(needsExitTime: true);
            {
                combatFsm.AddState("Wait", new WaitState(_context));

                combatFsm.SetStartState("Wait");
            }
            _rootSm.AddState("Combat", combatFsm);

            var defeatedFsm = new StateMachine(needsExitTime: true);
            {
                // defeatedFsm.AddState("Despawning", new DespawningState(_context));
            }
            _rootSm.AddState("Defeated", defeatedFsm);
            
            _rootSm.AddTransition("Intro", "Combat");
            _rootSm.AddTransition("Combat", "Defeated", _ => _context.health <= 0f, forceInstantly: true);

            _rootSm.SetStartState("Intro");
        }

        void Update()
        {
            _rootSm.OnLogic();
        }
    }
}