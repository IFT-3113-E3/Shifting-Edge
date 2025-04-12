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
                Debug.LogError("[BossMovementController] EntityMovementController component not found!");
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
        
        
        public void MoveTowards(Vector3 target, float force)
        {
            var dir = (target - transform.position).normalized;
            // _movement = dir * force;
            _mc.AddVelocity(dir * force);
        }
        
        public void LookAt(Vector3 target, float speed = 100f)
        {
            _targetDir = target - transform.position;
            _targetDir.y = 0;
            _targetDir.Normalize();
            HandleLookAt(speed);
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