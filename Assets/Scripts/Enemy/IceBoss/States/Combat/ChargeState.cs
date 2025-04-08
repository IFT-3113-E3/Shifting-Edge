using UnityEngine;
using UnityHFSM;

namespace Enemy.IceBoss.States.Combat
{
    public class ChargeState : StateBase
    {
        private readonly BossContext _ctx;
        
        public ChargeState(BossContext ctx) : base(true)
        {
            _ctx = ctx;
        }

        public override void OnEnter()
        {
            // Logic for entering the charge state
            _ctx.movementController.DashTo(_ctx.player.transform.position, _ctx.dashSpeed, _ctx.dashDuration,
                () =>
                {
                    // Transition to the next state after dashing
                    fsm.StateCanExit();
                });
        }

        public override void OnLogic()
        {
        }

        public override void OnExit()
        {
            _ctx.timeSinceLastAttack = 0f;
            _ctx.movementController.StopMovement();
        }
    }
}