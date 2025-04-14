using UnityEngine;
using UnityHFSM;

namespace Enemy.IceBoss.States.Combat
{
    public class ChargeState : StateBase
    {
        private enum Phase
        {
            Preparing,
            Punching,
            Done
        }

        private readonly BossContext _ctx;

        // Config
        private const float PrepareTime = 1.5f;
        private const float MaxChargeDistance = 15f;
        private const float ReachDistance = 5f;
        private const float ViewAngle = 100f;

        // State
        private Phase _phase = Phase.Preparing;
        private float _elapsedPrepareTime;
        private Vector3 _startPosition;
        private Vector3 _targetPosition;
        
        private Vector3 _tpTargetPosition;
        private Quaternion _tpTargetRotation;

        private int _punchCount = 0;
        
        public ChargeState(BossContext ctx) : base(true)
        {
            _ctx = ctx;
        }

        public override void OnEnter()
        {
            _punchCount = 0;
            _phase = Phase.Preparing;
            _elapsedPrepareTime = 0f;

            _ctx.animator.PreparePunch();
            _ctx.animator.SetChargingFlashEnabled(true);
        }

        public void UpdateTPTarget()
        {
            Vector3 tpPos = _ctx.movementController.GetRandomPointAroundPlayer(
                _ctx.player.transform.position,
                8f, 45f);
            Quaternion tpRot = Quaternion.LookRotation(
                _ctx.player.transform.position - tpPos);
            
            _tpTargetPosition = tpPos;
            _tpTargetRotation = tpRot;
        }
        public void TeleportAndPunchAgain()
        {
            Vector3 tpPos = _ctx.movementController.GetRandomPointAroundPlayer(
                _ctx.player.transform.position,
                8f, 45f);
            Quaternion tpRot = Quaternion.LookRotation(
                _ctx.player.transform.position - tpPos);
            
            _tpTargetPosition = tpPos;
            _tpTargetRotation = tpRot;
            _ctx.animator.TeleportWithExplosion(
                tpPos,
                tpRot,
                0.4f,
                () =>
                {
                    // _ctx.movementController.LookAt(_ctx.player.transform.position, 100000f);
                    _ctx.movementController.SyncTransform();
                    _ctx.animator.PreparePunch();
                    BeginPunch();
                });
        }

        public override void OnLogic()
        {
            switch (_phase)
            {
                case Phase.Preparing:
                    TickPreparation();
                    break;

                case Phase.Punching:
                    TickPunching();
                    break;

                case Phase.Done:
                {
                    UpdateTPTarget();
                    _ctx.animator.UpdateTarget(
                        _tpTargetPosition,
                        _tpTargetRotation);
                    break;
                }
            }
        }

        private void TickPreparation()
        {
            _ctx.movementController.LookAt(_ctx.player.transform.position, 300f);

            _elapsedPrepareTime += _ctx.dt;
            if (_elapsedPrepareTime >= PrepareTime)
            {
                BeginPunch();
            }
        }

        private void BeginPunch()
        {
            _ctx.animator.SetChargingFlashEnabled(false);
            _startPosition = _ctx.self.transform.position;
            var direction = (_ctx.player.transform.position - _startPosition).normalized;
            _targetPosition = _startPosition + direction * MaxChargeDistance;

            _phase = Phase.Punching;
        }

        private void TickPunching()
        {
            var currentPosition = _ctx.self.transform.position;

            if (IsPlayerInRange())
            {
                MoveToFinalPunchPosition();
                return;
            }

            if (HasReachedMaxDistance(currentPosition))
            {
                _ctx.animator.ReturnToIdle();
                _phase = Phase.Done;

                // try again if we still have time
                ExitOrTryAgain();
                return;
            }

            _ctx.movementController.MoveTowards(_targetPosition, _ctx.dashSpeed * _ctx.dt);
        }

        private bool IsPlayerInRange()
        {
            return _ctx.movementController.IsPointInRange(
                _ctx.player.transform.position,
                ViewAngle,
                ReachDistance);
        }

        private bool HasReachedMaxDistance(Vector3 currentPosition)
        {
            return Vector3.Distance(currentPosition, _startPosition) >= MaxChargeDistance - ReachDistance;
        }

        public void ExitOrTryAgain()
        {
            _punchCount++;
            if (_punchCount > 2)
            {
                fsm.StateCanExit();
            }
            else
            {
                TeleportAndPunchAgain();
            }
        }

        private void MoveToFinalPunchPosition()
        {
            _phase = Phase.Done;

            _ctx.animator.Punch(() =>
            {
                ExitOrTryAgain();
            });
            
            _ctx.movementController.LookAt(_ctx.player.transform.position, 100000f);

            _ctx.movementController.Mc.MoveTo(
                _ctx.movementController.GetNearestPointAroundPlayer(
                    _ctx.player.transform.position,
                    ReachDistance - 1f),
                1f,
                20f,
                1000f,
                () =>
                {
                    _ctx.movementController.LookAt(_ctx.player.transform.position, 100000f);
                });
        }

        public override void OnExit()
        {
            _ctx.timeSinceLastMeleeAttack = 0f;
            _ctx.attackHistory.Add(AttackType.Melee);
            _ctx.movementController.StopMovement();
        }
    }
}
