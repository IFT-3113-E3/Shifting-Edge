using System;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Enemy.IceBoss
{
    [RequireComponent(typeof(Rigidbody))]
    public class BossMovementController : MonoBehaviour
    {
        private EntityMovementController _mc;
        private Quaternion _interpolatedRotation;

        private Vector3 _targetDir;

        // private bool _active;
        private Action _onComplete;

        private Vector3 _startLerpPos;
        private AnimationCurve _easingCurve;

        private Vector3 _movement;
        private Vector3 _lookDirection;

        public EntityMovementController Mc => _mc;

        public float test = 1;

        public Quaternion TransientRotation => _interpolatedRotation;
        public Vector3 TransientForward => _interpolatedRotation * Vector3.forward;

        private void Start()
        {
            _mc = GetComponent<EntityMovementController>();
            if (_mc == null)
            {
                Debug.LogError(
                    "[BossMovementController] EntityMovementController component not found!");
            }

            _interpolatedRotation = transform.rotation;
        }

        public void StopMovement()
        {
            // _active = false;
            // _interpolatedRotation = transform.rotation;
            // _mc.SetVelocity(Vector3.zero);
            _movement = Vector3.zero;
            _lookDirection = Vector3.zero;
        }
        
        public void SyncTransform()
        {
            _interpolatedRotation = transform.rotation;
            _mc.SetVelocity(Vector3.zero);
        }

        public void MoveTowards(Vector3 target, float force)
        {
            var dir = (target - transform.position).normalized;
            dir.y = 0;
            dir.Normalize();
            // _movement = dir * force;
            _mc.AddVelocity(dir * force);
        }

        public void WalkTowards(Vector3 target)
        {
            var dir = (target - transform.position).normalized;
            dir.y = 0;
            dir.Normalize();
            _movement = dir;
        }
        
        public void ResetAtPointAndOrientation(Vector3 point, Quaternion rotation)
        {
            _interpolatedRotation = rotation;
            _mc.CancelVelocity();
            _mc.SetPosition(point);
            _mc.SetRotation(rotation);
        }
        
        public float DistanceTo(Vector3 target)
        {
            return Vector3.Distance(
                Vector3.ProjectOnPlane(transform.position, Vector3.up),
                Vector3.ProjectOnPlane(target, Vector3.up));
        }

        public void LookAt(Vector3 target, float speed = 100f)
        {
            _targetDir = target - transform.position;
            _targetDir.y = 0;
            _targetDir.Normalize();
            HandleLookAt(speed);
        }

        public bool IsPointInRange(Vector3 position, float viewAngle, float maxDistance)
        {
            var maxViewAngle = viewAngle / 2f;
            var viewDistance = maxDistance;

            var forward = TransientForward;

            var angleToTarget = Vector3.Angle(forward, (position - transform.position).normalized);
            var distanceToTarget = Vector3.Distance(transform.position, position);

            return angleToTarget <= maxViewAngle && distanceToTarget <= viewDistance;
        }

        public Vector3 GetNearestPointAroundPlayer(Vector3 playerPosition, float distance)
        {
            Vector3 toBoss = (transform.position - playerPosition);
            toBoss.y = 0f;

            if (toBoss == Vector3.zero)
            {
                return playerPosition + Vector3.forward * distance;
            }

            float angle = Mathf.Atan2(toBoss.z, toBoss.x) * Mathf.Rad2Deg;
            float snappedAngle = Mathf.Round(angle / 45f) * 45f;
            float rad = snappedAngle * Mathf.Deg2Rad;
            Vector3 snappedDir = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)).normalized;

            return playerPosition + snappedDir * distance;
        }
        
        // Calculate a random point around the player within a specified distance and a required minimum angle difference between the current and new direction
        public Vector3 GetRandomPointAroundPlayer(Vector3 playerPosition, float distance, float minAngleDifference)
        {
            // Ensure minAngleDifference is within a valid range.
            minAngleDifference = Mathf.Clamp(minAngleDifference, 0f, 180f);

            // Calculate the current direction from player to this object.
            Vector3 currentDirection = (transform.position - playerPosition).normalized;

            // Generate a random angle offset between the specified minimum and 180 degrees.
            float angleOffset = Random.Range(minAngleDifference, 180f);

            // Randomly decide whether to rotate clockwise or counter-clockwise.
            if (Random.value < 0.5f)
            {
                angleOffset = -angleOffset;
            }

            // Apply the rotation around the Y axis.
            Quaternion rotation = Quaternion.Euler(0, angleOffset, 0);
            Vector3 candidateDirection = rotation * currentDirection;

            // Return the new point at 'distance' away from the player.
            return playerPosition + candidateDirection * distance;
        }
        
        public Vector3 GetRandomPointAroundPlayer(Vector3 playerPosition, float distance)
        {
            Vector3 toBoss = (transform.position - playerPosition);
            toBoss.y = 0f;

            if (toBoss == Vector3.zero)
            {
                return playerPosition + Random.insideUnitSphere * distance;
            }

            float angle = Mathf.Atan2(toBoss.z, toBoss.x) * Mathf.Rad2Deg;
            float snappedAngle = Mathf.Round(angle / 45f) * 45f;
            float rad = snappedAngle * Mathf.Deg2Rad;
            Vector3 snappedDir = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)).normalized;

            return playerPosition + snappedDir * distance;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                var dir = -transform.position + GetRaycastMousePosition();
                _mc.AddVelocity(dir.normalized * test);
            }

            if (Input.GetKey(KeyCode.C))
            {
                var dir = -transform.position + GetRaycastMousePosition();
                _mc.AddVelocity(dir.normalized * test);
            }

            var inputs = new AICharacterInputs
            {
                MoveVector = _movement,
                LookVector = _lookDirection,
            };
            _mc.SetInputs(ref inputs);
            _movement = Vector3.zero;
            _lookDirection = Vector3.zero;
        }

        private Vector3 GetRaycastMousePosition()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition /
                                                   new Vector2(Screen.width, Screen.height) *
                                                   new Vector2(Camera.main.pixelWidth,
                                                       Camera.main.pixelHeight));
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                return hit.point;
            }

            return Vector3.zero;
        }

        private void HandleLookAt(float speed)
        {
            Quaternion targetRotation = Quaternion.LookRotation(_targetDir);


            _interpolatedRotation = Quaternion.RotateTowards(
                _interpolatedRotation,
                targetRotation,
                speed * Time.deltaTime);

            // snap rotation to 8 directions
            var currentRot = Quaternion.Euler(
                0,
                Mathf.Round(_interpolatedRotation.eulerAngles.y / 45f) * 45f,
                0);
            // rotate towards target
            // _rb.MoveRotation(currentRot);
            _lookDirection = currentRot * Vector3.forward;
        }
    }
}