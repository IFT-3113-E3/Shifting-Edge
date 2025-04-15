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
                _ctx.player.transform.position, 200f);
        }

        public override void OnExit()
        {
            _ctx.timeSinceLastThrow = 0f;
            _ctx.attackHistory.Add(AttackType.Ranged);
            _ctx.movementController.StopMovement();
            _ctx.animator.ReturnToIdle();
        }
    }
}