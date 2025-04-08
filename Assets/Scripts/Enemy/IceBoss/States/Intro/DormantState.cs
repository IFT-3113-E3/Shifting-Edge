using UnityEngine;
using UnityHFSM;

namespace Enemy.IceBoss.States.Intro
{
    public class DormantState : StateBase
    {
        private readonly BossContext _ctx;
        
        public DormantState(BossContext ctx) : base(false)
        {
            this._ctx = ctx;
        }

        public override void OnEnter()
        {
            // Logic for entering the dormant state
            _ctx.shouldActivate = false;
        }

        public override void OnLogic()
        {
            if (_ctx.shouldActivate)
            {
                // Logic for updating the dormant state
                // For example, check if the player is within a certain range
                // If so, transition to the spawning state
                fsm.StateCanExit(); // Finish this state to transition to the next one
            }
        }

        public override void OnExit()
        {
            Debug.Log("DormantState: Exit");
        }
    }
}