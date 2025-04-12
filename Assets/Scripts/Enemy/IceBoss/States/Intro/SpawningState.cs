using UnityEngine;
using UnityHFSM;

namespace Enemy.IceBoss.States.Intro
{
    public class SpawningState : StateBase
    {
        private bool _isSpawning = false;

        private readonly BossContext _ctx;
        
        public SpawningState(BossContext ctx) : base(true)
        {
            _ctx = ctx;
        }

        public override void OnEnter()
        {
            _isSpawning = true;

            Debug.Log("SpawningState: Enter");
            
            // Setup the camera for the fight
            _ctx.orbitCamera.AddAdditionalTarget(_ctx.self.transform);
            
            // Start the fracture effect
            if (_ctx.animator)
            {
                _ctx.animator.AssembleAndSpawn(_ctx.spawnPoint.position, _ctx.spawnPoint.rotation,
                    () =>
                    {
                        _isSpawning = false;
                        // Transition to the next state after spawning
                        fsm.StateCanExit();
                    });
            }
            else
            {
                Debug.LogWarning("FractureEffect component not found on the boss.");
            }
        }

        public override void OnLogic()
        {

        }

        public override void OnExit()
        {
            _ctx.animator?.EnsureBossIsVisibleAndFragmentsAreDisabled();
        }
    }
}