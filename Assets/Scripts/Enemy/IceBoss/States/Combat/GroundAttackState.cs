using UnityHFSM;

namespace Enemy.IceBoss.States.Combat
{
    public class GroundAttackState : StateBase
    {
        private readonly BossContext _ctx;

        public GroundAttackState(BossContext ctx) : base(true)
        {
            _ctx = ctx;
        }

        public override void OnEnter()
        {
            _ctx.animator.GroundAttack(() =>
            {
                fsm.StateCanExit();
            });
        }

        public override void OnLogic()
        {
            
        }

        public override void OnExit()
        {
            // Logic for exiting the ground attack state
            _ctx.timeSinceLastGroundAttack = 0f;
            _ctx.attackHistory.Add(AttackType.Ground);
        }
    }
}