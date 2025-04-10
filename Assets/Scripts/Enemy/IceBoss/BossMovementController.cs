using System;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Enemy.IceBoss
{
    public enum MovementType
    {
        None,
        Floating,
        Dashing,
        Orbiting,
        LerpTo,
        LookAtTarget,
    }

    [RequireComponent(typeof(Rigidbody))]
    public class BossMovementController : MonoBehaviour
    {
        public bool enableIdleFloat = false;

        [Header("Floating Idle Motion")]
        public float floatAmplitude = 0.5f;
        public float floatFrequency = 1f;

        public float idleDriftSpeed = 0.5f;
        public float idleDriftRadius = 0.25f;

        private Vector3 _idleBasePosition;
        private Vector3 _idleDriftOffset;
        private float _idleTime;

        
        private Rigidbody _rb;
        private Quaternion _interpolatedRotation;

        private MovementType _currentMode = MovementType.None;
        private Vector3 _target;
        private Vector3 _direction;
        private float _speed;
        private float _dashForce;
        private float _duration;
        private float _timer;

        private bool _active;
        private Action _onComplete;

        private Vector3 _startLerpPos;
        private AnimationCurve _easingCurve;

        private void Start()
        {
            _rb = GetComponent<Rigidbody>();
            if (_rb == null)
            {
                Debug.LogError("[BossMovementController] Rigidbody component not found!");
            }
            _interpolatedRotation = transform.rotation;
        }

        public void StopMovement()
        {
            _active = false;
            enableIdleFloat = true;
            _idleBasePosition = transform.position;
            _idleDriftOffset = transform.position;
            _interpolatedRotation = transform.rotation;
            _idleTime = Random.Range(0f, 100f); // prevent identical sync for clones 
            _rb.linearVelocity = Vector3.zero;
        }

        public void FloatTo(Vector3 target, float speed, float duration = -1f,
            Action onComplete = null)
        {
            _active = true;
            enableIdleFloat = false;

            _currentMode = MovementType.Floating;
            _target = target;
            _speed = speed;
            _duration = duration;
            _onComplete = onComplete;
            _timer = 0f;
            _active = true;
        }

        public void DashTo(Vector3 target, float force, float duration = 0.3f,
            Action onComplete = null)
        {
            _active = true;
            enableIdleFloat = false;

            _currentMode = MovementType.Dashing;
            _direction = (target - transform.position).normalized;
            _dashForce = force;
            _duration = duration;
            _timer = 0f;
            _onComplete = onComplete;
            _active = true;
        }

        public void LerpTo(Vector3 target, float duration, AnimationCurve easing,
            Action onComplete = null)
        {
            _active = true;
            enableIdleFloat = false;

            _currentMode = MovementType.LerpTo;
            _startLerpPos = transform.position;
            _target = target;
            _duration = duration;
            _easingCurve = easing;
            _timer = 0f;
            _onComplete = onComplete;
            _active = true;
        }
        
        public void LookAt(Vector3 target, float duration = 0.5f,
            Action onComplete = null)
        {
            enableIdleFloat = true;
            _currentMode = MovementType.LookAtTarget;
            _target = target;
            _duration = duration;
            _timer = 0f;
            _onComplete = onComplete;
            _active = true;
        }

        void FixedUpdate()
        {
            if (!_active) return;

            _timer += Time.fixedDeltaTime;

            switch (_currentMode)
            {
                case MovementType.Floating:
                    HandleFloating();
                    break;
                case MovementType.Dashing:
                    HandleDash();
                    break;
                case MovementType.LerpTo:
                    HandleLerp();
                    break;
                case MovementType.LookAtTarget:
                    HandleLookAt();
                    break;
            }

            if (_duration > 0f && _timer >= _duration)
            {
                StopMovement();
                _onComplete?.Invoke();
            }
        }
        
        void LateUpdate()
        {
            if (!enableIdleFloat)
                return;

            _idleTime += Time.deltaTime;

            // Vertical sine offset (levitation)
            float yOffset = Mathf.Sin(_idleTime * floatFrequency * Mathf.PI * 2f) * floatAmplitude;

            // Lazy horizontal drifting
            Vector3 driftTarget = _idleBasePosition +
                                  new Vector3(Mathf.Sin(_idleTime * 0.3f), 0f, Mathf.Cos(_idleTime * 0.4f)) * idleDriftRadius;

            _idleDriftOffset = Vector3.Lerp(_idleDriftOffset, driftTarget, Time.deltaTime * idleDriftSpeed);

            // Combine movement + float
            transform.position = _idleDriftOffset + Vector3.up * yOffset;
        }


        private void HandleFloating()
        {
            Vector3 dir = (_target - transform.position).normalized;
            _rb.MovePosition(_rb.position + dir * (_speed * Time.fixedDeltaTime));
        }

        private void HandleDash()
        {
            _rb.linearVelocity = _direction * _dashForce;
        }

        private void HandleLerp()
        {
            float t = Mathf.Clamp01(_timer / _duration);
            float easedT = _easingCurve.Evaluate(t);
            Vector3 newPos = Vector3.Lerp(_startLerpPos, _target, easedT);
            _rb.MovePosition(newPos);
        }
        
        private void HandleLookAt()
        {
            Vector3 targetDir = _target - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(targetDir);
            
            
            _interpolatedRotation = Quaternion.RotateTowards(
                _interpolatedRotation,
                targetRotation,
                100f * Time.fixedDeltaTime);
            
            // snap rotation to 8 directions
            var currentRot = Quaternion.Euler(
                0,
                Mathf.Round(_interpolatedRotation.eulerAngles.y / 45f) * 45f,
                0);
            // rotate towards target
            _rb.MoveRotation(currentRot);
        }
    }
}