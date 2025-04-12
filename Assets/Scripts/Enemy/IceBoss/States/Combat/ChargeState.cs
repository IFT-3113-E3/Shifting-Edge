using UnityEngine;
using UnityHFSM;

namespace Enemy.IceBoss.States.Combat
{
    public class ChargeState : StateBase
    {
        private readonly BossContext _ctx;
     
        private float _prepareTime = 1.5f;
        private float _prepareTimeElapsed = 0f;
        private bool _hasStartedPreparing = false;
        private bool _preparingPunch = false;
        private bool _punching = false;
        
        private bool _reachedTarget = false;
        
        private Vector3 _startPosition;
        private Vector3 _targetPosition;
        private float _maxDistance = 15f;
        
        public ChargeState(BossContext ctx) : base(true)
        {
            _ctx = ctx;
        }

        public override void OnEnter()
        {

            _hasStartedPreparing = false;
            _preparingPunch = true;
            _punching = false;
            _prepareTimeElapsed = 0f;
        }

        private void ChargeInit()
        {
                                
            _ctx.movementController.LookAt(
                _ctx.player.transform.position, 100000f);
            
            _reachedTarget = false;
            _startPosition = _ctx.self.transform.position;
            
            var diff = _ctx.player.transform.position - _startPosition;
            
            var dir = diff.normalized;
            _targetPosition = _startPosition + dir * _maxDistance;
        }

        public override void OnLogic()
        {
            if (_preparingPunch)
            {
                if (!_hasStartedPreparing)
                {
                    Debug.Log("Preparing punch");
                    _ctx.animator.PreparePunch();
                    _ctx.animator.SetFlashEnabled(true);
                    _hasStartedPreparing = true;
                }
                _ctx.movementController.LookAt(
                    _ctx.player.transform.position);
                _prepareTimeElapsed += _ctx.dt;
                if (_prepareTimeElapsed >= _prepareTime)
                {
                    Debug.Log("Punching");
                    _punching = true;
                    _preparingPunch = false;
                    _ctx.animator.SetFlashEnabled(false);
                    ChargeInit();
                }
                
                return;
            }
            
            if (_punching)
            {
                DashAndPunch();
            }
        }

        private void DashAndPunch()
        {
            if (_reachedTarget)
            {
                return;
            };
            
            var reachDistance = 5f;
            var viewAngle = 100f;
            var dstFromStart = Vector3.Distance(_ctx.self.transform.position, _startPosition);
            
            if (IsPointInRange(_ctx.player.transform.position, viewAngle, reachDistance))
            {
                Debug.Log("Player is in range");
                _reachedTarget = true;
                _ctx.movementController.Mc.MoveTo(GetNearestPointAroundPlayer(
                        _ctx.player.transform.position, reachDistance - 1f), 1f, 20f, 1000f,
                    () =>
                    {
                        _ctx.movementController.LookAt(_ctx.player.transform.position, 100000f);
                        _ctx.animator.Punch(() =>
                        {
                            fsm.StateCanExit();
                        });
                    });
                return;
            }

            if (dstFromStart >= _maxDistance - reachDistance)
            {
                Debug.Log("Reached max distance");
                _ctx.animator.ReturnToIdle();
                _reachedTarget = true;
                fsm.StateCanExit();
                return;
            }

            _ctx.movementController.MoveTowards(
                _targetPosition,
                _ctx.dashSpeed * _ctx.dt);
        }


        private bool IsPointInRange(Vector3 position, float viewAngle, float maxDistance)
        {
            var maxViewAngle = viewAngle / 2f;
            var viewDistance = maxDistance;

            var forward = _ctx.movementController.TransientForward;

            var angleToTarget = Vector3.Angle(forward, (position - _ctx.self.transform.position).normalized);
            var distanceToTarget = Vector3.Distance(_ctx.self.transform.position, position);

            return angleToTarget <= maxViewAngle && distanceToTarget <= viewDistance;
        }
        
        // Finds the nearest point of the boss that is in a 45 degree angle snapped to the player
        // so one of 8 directions around the player in world space
        public Vector3 GetNearestPointAroundPlayer(Vector3 playerPosition, float distance)
        {
            Vector3 toBoss = (_ctx.self.transform.position - playerPosition);
            toBoss.y = 0f; // Ignore vertical

            if (toBoss == Vector3.zero)
            {
                return playerPosition + Vector3.forward * distance; // Default direction
            }

            // Convert direction to angle (0-360)
            float angle = Mathf.Atan2(toBoss.z, toBoss.x) * Mathf.Rad2Deg;

            // Snap to nearest 45-degree angle
            float snappedAngle = Mathf.Round(angle / 45f) * 45f;

            // Convert angle back to direction
            float rad = snappedAngle * Mathf.Deg2Rad;
            Vector3 snappedDir = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)).normalized;

            // Return point at distance in that direction
            return playerPosition + snappedDir * distance;
        }

        public override void OnExit()
        {
            _ctx.timeSinceLastAttack = 0f;
            _ctx.movementController.StopMovement();
        }
    }
}