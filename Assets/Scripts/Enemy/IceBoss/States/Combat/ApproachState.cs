using UnityHFSM;

namespace Enemy.IceBoss.States.Combat
{
    public class ApproachState : StateBase
    {
        private readonly BossContext _ctx;

        public ApproachState(BossContext ctx) : base(false)
        {
            _ctx = ctx;
        }

        public override void OnEnter()
        {
        }

        public override void OnLogic()
        {
            _ctx.movementController.LookAt(_ctx.player.transform.position, 300f);
            _ctx.movementController.WalkTowards(_ctx.player.transform.position);
        }

        public override void OnExit()
        {
        }
    }
}