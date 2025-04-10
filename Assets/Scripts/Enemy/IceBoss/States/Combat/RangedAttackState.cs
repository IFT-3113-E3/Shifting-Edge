using UnityHFSM;

namespace Enemy.IceBoss.States.Combat
{
    public class RangedAttackState : StateBase
    {
        private readonly BossContext _ctx;

        public RangedAttackState(BossContext ctx) : base(true)
        {
            _ctx = ctx;
        }

        public override void OnEnter()
        {
            _ctx.animator.ThrowSpike(() =>
            {
                fsm.StateCanExit();
            });
        }

        public override void OnLogic()
        {
            _ctx.movementController.LookAt(
                _ctx.player.transform.position);
        }

        public override void OnExit()
        {
            // Logic for exiting the ranged attack state
            _ctx.timeSinceLastAttack = 0f;
            _ctx.timeSinceLastThrow = 0f;
            _ctx.movementController.StopMovement();
        }
    }
}