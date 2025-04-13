using UnityEngine;
using UnityHFSM;

namespace Enemy.IceBoss.States.Combat
{
    public class MoveIntoPositionState : StateBase
    {
        private readonly BossContext _ctx;
        private Vector3 _targetPosition;

        private readonly float _desiredDistance;
        private const float positionVariance = 1.5f;
        private const float arcAngleDegrees = 60f; // e.g., ±30°

        public MoveIntoPositionState(BossContext ctx, float distance) : base(true)
        {
            _desiredDistance = distance;
            _ctx = ctx;
        }

        public override void OnEnter()
        {
            Vector3 bossPos = _ctx.self.transform.position;
            Vector3 playerPos = _ctx.player.transform.position;

            Vector3 toBoss = (bossPos - playerPos).normalized;

            float angleOffset = Random.Range(-arcAngleDegrees / 2f, arcAngleDegrees / 2f);
            Quaternion rotation = Quaternion.Euler(0f, angleOffset, 0f);
            Vector3 offsetDirection = rotation * toBoss;

            float distance = _desiredDistance + Random.Range(-positionVariance, positionVariance);

            _targetPosition = playerPos + offsetDirection * distance;
        }

        public override void OnLogic()
        {
            _ctx.movementController.LookAt(_targetPosition, 300f);
            _ctx.movementController.WalkTowards(_targetPosition);
            
            if (_ctx.movementController.DistanceTo(_targetPosition) < 0.5f)
            {
                fsm.StateCanExit();
            }
        }

        public override void OnExit()
        {
        }
    }
}