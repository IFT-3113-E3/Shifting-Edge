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
            _ctx.movementController.LookAt(_ctx.player.transform.position);
        }

        public override void OnExit()
        {
            Debug.Log("WaitState: Exit");
            _ctx.movementController.StopMovement();
        }
    }
}