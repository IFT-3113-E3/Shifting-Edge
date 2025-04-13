using UnityHFSM;

namespace Enemy.IceBoss.States.Combat
{
    public class GroundAttackState : StateBase
    {
        private readonly BossContext _ctx;
        
        private float _prepareTime = 0.5f;
        private float _prepareTimeElapsed = 0f;
        private bool _preparingGroundAttack = false;
        private bool _groundAttacking = false;
        
        private int _attackCount = 0;
        

        public GroundAttackState(BossContext ctx) : base(true)
        {
            _ctx = ctx;
        }

        public override void OnEnter()
        {
            _attackCount = 0;
            ResetForNewAttack();
        }

        private void ResetForNewAttack()
        {
            _prepareTimeElapsed = 0f;
            _preparingGroundAttack = true;
            _groundAttacking = false;
            _ctx.animator.SetFlashEnabled(true);
        }

        public override void OnLogic()
        {
            if (_preparingGroundAttack)
            {
                _ctx.movementController.LookAt(_ctx.player.transform.position, _ctx.lookAtSpeed);
                
                _prepareTimeElapsed += _ctx.dt;
                if (_prepareTimeElapsed >= _prepareTime)
                {
                    _preparingGroundAttack = false;
                    _groundAttacking = true;
                    _ctx.animator.SetFlashEnabled(false);

                    _ctx.animator.GroundAttack(() =>
                    {
                        if (_attackCount > 3)
                        {
                            fsm.StateCanExit();
                        }
                        else
                        {
                            _attackCount++;
                            ResetForNewAttack();
                        }
                    });
                }
            }
            
            
        }

        public override void OnExit()
        {
            // Logic for exiting the ground attack state
            _ctx.timeSinceLastGroundAttack = 0f;
            _ctx.attackHistory.Add(AttackType.Ground);
        }
    }
}