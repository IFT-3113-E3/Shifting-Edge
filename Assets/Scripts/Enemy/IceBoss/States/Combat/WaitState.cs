using UnityEngine;
using UnityHFSM;

namespace Enemy.IceBoss.States.Combat
{
    public class WaitState : StateBase
    {
        private readonly BossContext _ctx;
        public WaitState(BossContext ctx) : base(true)
        {
            _ctx = ctx;
        }

        public override void OnEnter()
        {
            if (!_ctx.hasJustTeleported)
            {
                _ctx.waitTimer = 0f;
            }
            _ctx.hasJustTeleported = false;
        }

        public override void OnLogic()
        {
            _ctx.waitTimer += Time.deltaTime;
            if (_ctx.waitTimer >= _ctx.attackWaitCooldown)
            {
                fsm.StateCanExit();
            }
            _ctx.movementController.LookAt(_ctx.player.transform.position);
        }

        public override void OnExit()
        {
        }
    }
}