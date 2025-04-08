using UnityEngine;
using UnityHFSM;

namespace Enemy.IceBoss.States.Combat
{
    public class WaitState : StateBase
    {
        private float _waitTime = 0f;
        
        private readonly BossContext _ctx;
        
        public WaitState(BossContext ctx) : base(false)
        {
            _ctx = ctx;
        }

        public override void OnEnter()
        {
        }

        public override void OnLogic()
        {
            // Logic for waiting
            _waitTime += Time.deltaTime;

            if (_waitTime >= _ctx.attackCooldown)
            {
                // Transition to the next state (e.g., attack state)
                fsm.StateCanExit();
            }
        }
    }
}